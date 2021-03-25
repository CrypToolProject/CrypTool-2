package enigma;

import java.util.ArrayList;
import java.util.Arrays;

class SubGraphItem { // edge (link) between two letters

    //todo - setters (for dist) and getters for all
    final int pos;    // position in ciphertext
    final byte l1;     // left side of link
    final byte l2;     // right side of link
    int dist;   // used to order the graph by distance (bfs)

    SubGraphItem(int nPos, byte nL1, byte nL2, int nDist) {
        pos = nPos;
        l1 = nL1;
        l2 = nL2;
        dist = nDist;
    }

}

class SubGraph { // connected subgraph (undirected)

    public final ArrayList<SubGraphItem> items = new ArrayList<>();   // links/edges
    public int closures;                                                   // number of loops

    public void addItem(SubGraphItem item) {
        items.add(item);
    }
}

public class BombeMenu {

    private static final int MAXMENUL = 100;
    private static final byte UNASSIGNED = -1;

    // todo - add setters and getters
    public final byte[] crib = new byte[BombeCrib.MAXCRIBL];
    public final int cribLen;
    public final int cribStartPos;

    private final SubGraph[] subGraphs = new SubGraph[BombeMenu.MAXMENUL];
    public int nSubgraphs;

    public int totalClosures;
    public int totalItems;
    public Double score;

    BombeMenu(int nPos, int nCrlen, byte[] nCrib) {

        cribStartPos = nPos;
        cribLen = nCrlen;
        System.arraycopy(nCrib, 0, crib, 0, cribLen);

        nSubgraphs = 0;

        score = BombeCrib.BADSCORE;
        totalItems = 0;
        nSubgraphs = 0;

    }


    // Sort the subgraphs according to their score (the most discriminative with highest score first)
    public void sortSubgraphs(boolean print) {

        if (nSubgraphs < 2)
            return;
        boolean changed;
        do {
            changed = false;
            for (int i = 0; i < (nSubgraphs - 1); i++)
                if (BombeCrib.score(subGraphs[i].closures, subGraphs[i].items.size()) >
                        BombeCrib.score(subGraphs[i + 1].closures, subGraphs[i + 1].items.size())) {

                    SubGraph temp = subGraphs[i];
                    subGraphs[i] = subGraphs[i + 1];
                    subGraphs[i + 1] = temp;

                    changed = true;
                }
        } while (changed);


        if (print) {
            System.out.printf("Sorting subgraphs for crib at position %d(to optimize validity checks on ciphertext)\n", cribStartPos);
            System.out.printf("Total %d subgraphs found, total %d closures and %d links, Turing score: %.2f \n",
                    nSubgraphs, totalClosures, totalItems, score);

            for (int subgraph = 0; subgraph < nSubgraphs; subgraph++) {

                System.out.printf("Subgraph #%d with %d Closures, %d Links\n",
                        subgraph, subGraphs[subgraph].closures,
                        subGraphs[subgraph].items.size());
                for (SubGraphItem item : subGraphs[subgraph].items)
                    System.out.printf("(%d) Link %s->%s at pos %d\n",
                            subgraph, Utils.getChar(item.l1),
                            Utils.getChar(item.l2), item.pos);
            }
            System.out.print("\n");

        }


    }


    // The main Bombe stopping algorithm - may be called recursively if more than one subgraph.
    // Scrambler means the substitution alphabets for each position, for the given settings, taking into
    // account the rotors-to-reflector and back path. Does not take into account stecker plugs, as the purpose of the
    // Turing Bombe is not only to check for a stop, but also to retrieve most or all of the stecker plugs.
    public boolean testIfBombsStops(int sg, byte[] scramblerLookupFlat,
                                    byte[] stbAssumed, byte[] stbStrength, boolean print) {


        byte firstLetter = subGraphs[sg].items.get(0).l1;

        ArrayList<Byte> pairedLettersToCheck = new ArrayList<>();

        if (stbAssumed[firstLetter] == UNASSIGNED) {
            // This is what we get when called externally (not a recursive call)
            // always try first self steckered for the first letter - it may or may not be already defined as such
            pairedLettersToCheck.add(firstLetter);
            //then add all unassigned
            for (byte pairedLetter = 0; pairedLetter < 26; pairedLetter++) {
                if (pairedLetter == firstLetter)
                    continue;
                // add the letters not yet mapped
                if (stbAssumed[pairedLetter] == UNASSIGNED)
                    pairedLettersToCheck.add(pairedLetter);
            }
        } else {
            // This is what we get when called internally (recursion for subgraph)
            // stick with what we have in currently assumed stb (1 option)
            pairedLettersToCheck.add(stbAssumed[firstLetter]);
        }

        boolean valid;

        // out of the loop for performance
        byte stbAssumedTemp[] = new byte[26];  // Assumed stecker plugs so far
        byte stbStrengthTemp[] = new byte[26]; // For each letter and its assumed stecker mapping,
        // how may times the assumption has been made.

        // iterate on all assumptions for first letter
        for (byte pairedLetter : pairedLettersToCheck) {

            // For printing...
            String assumptionS = "" + Utils.getChar(pairedLetter) + Utils.getChar(firstLetter);

            // create local copies, and copy back only if returning valid
            System.arraycopy(stbStrength, 0, stbStrengthTemp, 0, 26);
            System.arraycopy(stbAssumed, 0, stbAssumedTemp, 0, 26);

            if (stbAssumedTemp[firstLetter] == UNASSIGNED) {

                stbAssumedTemp[pairedLetter] = firstLetter;
                stbAssumedTemp[firstLetter] = pairedLetter;

                if (print)
                    System.out.printf("\n(SG %d) VALIDITY CHECK - Assuming [%s]\n\n", sg, assumptionS);

            } else if (stbAssumedTemp[firstLetter] == pairedLetter) {

                // do nothing
                if (print)
                    System.out.printf("\n(SG %d) VALIDITY CHECK - Assumption [%s] - Already assumed from previous subgraph\n\n",
                            sg, assumptionS);

            } else { // should never happen!
                System.out.printf("\n(SG %d) VALIDITY CHECK - CRITICAL ERROR Assumption [%s] contradicts previous settings for first letter\n\n",
                        sg, assumptionS);
                return false;
            }


            valid = true;
            for (SubGraphItem item : subGraphs[sg].items) {

                // EnigmaIn =>[STB]=> ScramblerIn =>[SCRAMBLER]=> ScramblerOut =>[STB]=> EnigmaOut

                byte enigmaIn;
                byte enigmaOut;
                if (stbAssumedTemp[item.l1] != UNASSIGNED) {
                    enigmaIn = item.l1;
                    enigmaOut = item.l2;
                } else if (stbAssumedTemp[item.l2] != UNASSIGNED) {
                    enigmaIn = item.l2;
                    enigmaOut = item.l1;
                } else {
                    System.out.printf("(SG: %d) VALIDITY CHECK - CRITICAL ERROR - SUBGRAPH ITEMS NOT SORTED!\n", sg);
                    return false;
                }

                byte scramblerIn = stbAssumedTemp[enigmaIn];
                byte scramblerOut = scramblerLookupFlat[(item.pos << 5) + scramblerIn];


                //time consuming - compute only if to print
                if (print) {

                    System.out.printf("\n(SG %d) CHECKING: %s->%s  at position %3d, Current STB: [%-27s %-12s]: [%s] <-STB-> [%s] <-Rotors-UKW-Rotors-> [%s] <-STB-> [%s]\n",
                            sg,
                            Utils.getChar(item.l1),
                            Utils.getChar(item.l2),
                            item.pos,
                            stbPairsString(stbAssumedTemp, null),
                            stbSelfsString(stbAssumedTemp, null),
                            Utils.getChar(enigmaIn),
                            Utils.getChar(scramblerIn),
                            Utils.getChar(scramblerOut),
                            Utils.getChar(enigmaOut));
                }


                if ((stbAssumedTemp[enigmaOut] == UNASSIGNED) &&  (stbAssumedTemp[scramblerOut] == UNASSIGNED)) {
                    if (enigmaOut == scramblerOut) {
                        if (print) {
                            System.out.printf("(SG %d) PASSED - Self [%s] valid - %s was undefined in STB\n",
                                    sg, Utils.getChar(scramblerOut), Utils.getChar(scramblerOut));
                        }
                        stbAssumedTemp[scramblerOut] = scramblerOut;


                    } else {
                        if (print) {
                            System.out.printf("(SG %d) PASSED - Pair [%s%s] valid - Both %s and %s were undefined in STB\n",
                                    sg, Utils.getChar(scramblerOut), Utils.getChar(enigmaOut), Utils.getChar(scramblerOut), Utils.getChar(enigmaOut));
                        }

                        stbAssumedTemp[enigmaOut] = scramblerOut;
                        stbAssumedTemp[scramblerOut] = enigmaOut;

                    }

                } else if ((stbAssumedTemp[scramblerOut] != UNASSIGNED) && (stbAssumedTemp[scramblerOut] != enigmaOut)) {

                    if (print) {
                        if (enigmaOut == scramblerOut)
                            System.out.printf("(SG %d) FAILED - Assumption [%s] rejected - Self [%s] contradicts STB\n",
                                    sg, assumptionS, Utils.getChar(scramblerOut));
                        else
                            System.out.printf("(SG %d) FAILED - Assumption [%s] rejected - Forward Pair [%s%s] contradicts STB\n",
                                    sg, assumptionS, Utils.getChar(scramblerOut), Utils.getChar(enigmaOut));
                    }

                    valid = false;
                    break;

                } else if ((stbAssumedTemp[enigmaOut] != UNASSIGNED) && (stbAssumedTemp[enigmaOut] != scramblerOut)) {

                    if (print) {
                        if (enigmaOut == scramblerOut)
                            System.out.printf("(SG %d) FAILED - Assumption [%s] rejected - Self [%s] contradicts STB\n",
                                    sg, assumptionS, Utils.getChar(scramblerOut));
                        else
                            System.out.printf("(SG %d) FAILED - Assumption [%s] rejected - Backward Pair [%s%s] contradicts STB\n",
                                    sg, assumptionS, Utils.getChar(scramblerOut), Utils.getChar(enigmaOut));
                    }

                    valid = false;
                    break;

                } else {

                    if (enigmaOut == scramblerOut) {
                        if (print)
                            System.out.printf("(SG %d) PASSED - Self [%s] confirmed for STB\n",
                                    sg, Utils.getChar(scramblerOut));

                        stbStrengthTemp[enigmaOut]++;

                    } else {

                        if (print)
                            System.out.printf("(SG %d) PASSED - Pair [%s%s] confirmed for STB\n",
                                    sg, Utils.getChar(scramblerOut), Utils.getChar(enigmaOut));

                        // count only in one direction (the lowest of the pair)
                        if (enigmaOut < scramblerOut)
                            stbStrengthTemp[enigmaOut]++;
                        else
                            stbStrengthTemp[scramblerOut]++;
                    }
                }
            }

            // No conflicts so far.
            if (valid) {

                if (print)
                    System.out.printf("\n(SG %d) PASS - Subgraph %d check complete\n", sg, sg);

                // Any more subgraph to check (recursively).
                if (sg < (nSubgraphs - 1)) {

                    if (print)
                        System.out.printf("CHECK INTO SG %d \n", sg + 1);


                    valid = testIfBombsStops(sg + 1, scramblerLookupFlat, stbAssumedTemp, stbStrengthTemp, print);

                    if (print) {
                        if (!valid)
                            System.out.printf("FAILED BACK FROM SG %d \n", sg + 1);
                        else
                            System.out.printf("SUCCESS BACK FROM SG %d \n", sg + 1);
                    }

                }
                // Subgraphs also validated
                if (valid) {


                    if (print) {
                        // Are we at top level graph?
                        if (sg == 0)
                            System.out.printf("\nCOMPLETE - STB [%s %s] - Pos %d - Crib >%s< - Tested %d subgraphs\n\n",
                                    stbPairsString(stbAssumedTemp, stbStrengthTemp),
                                    stbSelfsString(stbAssumedTemp, stbStrengthTemp),
                                    cribStartPos, Utils.getString(crib, cribLen), nSubgraphs);
                        else
                            System.out.printf("(SG %d) COMPLETE - STB [%s %s] \n",
                                    sg,
                                    stbPairsString(stbAssumedTemp, stbStrengthTemp),
                                    stbSelfsString(stbAssumedTemp, stbStrengthTemp));
                    }

                    // Copy back the values to return.
                    System.arraycopy(stbAssumedTemp, 0, stbAssumed, 0, 26);
                    System.arraycopy(stbStrengthTemp, 0, stbStrength, 0, 26);

                    return true;
                }


            }

        }

        // check nothing spoiled is returned
        Key.checkStecker(stbAssumed, "VALIDITY TEST - SG =" + sg + " RETURNING FAILURE (GLOBAL)");

        return false;

    }


    public void addSubgraph(int[][] links, int nClosures, int[] letterUsage, boolean print) {


        SubGraphItem tempItems[] = new SubGraphItem[MAXMENUL];
        int numberOfItems = 0;

        SubGraph subGraph = subGraphs[nSubgraphs] = new SubGraph();

        subGraph.closures = nClosures;
        totalClosures += subGraph.closures;

        if (print) {

            StringBuilder letterUsageS = new StringBuilder();
            StringBuilder letterUsagePlusS = new StringBuilder();
            for (int j = 0; j < 26; j++) {
                if (letterUsage[j] > 0)
                    letterUsageS.append(Utils.getChar(j));
                if (letterUsage[j] > 1)
                    letterUsagePlusS.append(Utils.getChar(j));
            }
            System.out.printf("Found menu subgraph (%d total) using letters [%s] - Letters [%s] traversed more than once\n",
                    nSubgraphs, letterUsageS.toString(), letterUsagePlusS.toString());
        }


        for (byte i = 0; i < 26; i++) {
            for (byte j = (byte) (i + 1); j < 26; j++) {
                if ((letterUsage[i] == 0) || (letterUsage[j] == 0))
                    continue;
                if (links[i][j] != -1) {
                    tempItems[numberOfItems++] = new SubGraphItem(links[i][j], i, j, -1);
                } else if (links[j][i] != -1) {
                    tempItems[numberOfItems++] = new SubGraphItem(links[j][i], j, i, -1);
                }


            }
        }      
        
       /* 
       if (print) { 
            for (int n = 0; n < sgNItems; n++)
                System.out.printf(" BEFORE SORT: %s->%s AT %d\n",Utils.getChar(tempItem[n].l1), Utils.getChar(tempItem[n].l2),tempItem[n].pos);
            System.out.printf(" END OF MENU\n");
       } 
         * 
         */

        for (int i = 0; i < numberOfItems; i++)
            tempItems[i].dist = 1000;

        int letterDist[] = new int[26];
        Arrays.fill(letterDist, 1000);

        letterDist[tempItems[0].l1] = 0;
        for (int dist = 0; dist < 26; dist++) {
            for (int i = 0; i < numberOfItems; i++) {
                letterDist[tempItems[i].l1] =
                        Math.min(letterDist[tempItems[i].l1], letterDist[tempItems[i].l2] + 1);
                letterDist[tempItems[i].l2] =
                        Math.min(letterDist[tempItems[i].l2], letterDist[tempItems[i].l1] + 1);
                tempItems[i].dist = Math.min(tempItems[i].dist, letterDist[tempItems[i].l1]);
                tempItems[i].dist = Math.min(tempItems[i].dist, letterDist[tempItems[i].l2]);

            }
        }

 /*
         if (print) { 

             for (int n = 0; n < sgNItems; n++)
                System.out.printf(" AFTER DIST: %s->%s AT %d DIST %d (%d,%d)\n",
                    Utils.getChar(tempItems[n].l1), Utils.getChar(tempItems[n].l2),
                          tempItems[n].pos,
                          tempItems[n].dist,
                          letterDist[tempItems[n].l1],
                          letterDist[tempItems[n].l2]);
                    
            System.out.printf("\n");  
         }           
*/

        for (int dist = 0; dist < 26; dist++) {
            for (int i = 0; i < numberOfItems; i++) {
                if (tempItems[i].dist == dist) {
                    subGraph.addItem(tempItems[i]);
                }
            }
        }

        totalItems += subGraph.items.size();
        nSubgraphs++;

        if (print) {
            for (SubGraphItem item : subGraph.items)
                System.out.printf("(%d) [Dist %d] Link %s->%s AT %d\n",
                        nSubgraphs - 1,
                        item.dist + 1,
                        Utils.getChar(item.l1),
                        Utils.getChar(item.l2),
                        item.pos);

            System.out.printf("Summary for subgraph %d (pos %d): %d Closures, %d Links\n",
                    nSubgraphs - 1, this.cribStartPos, subGraph.closures,
                    subGraph.items.size());

        }

    }

    private String stbPairsString(byte[] assumedStecker, byte[] strength) {
        StringBuilder stbs = new StringBuilder();
        for (int k = 0; k < 26; k++) {
            int s = assumedStecker[k];
            if (k < s) {
                stbs.append(Utils.getChar(k)).append(Utils.getChar(s));
                if ((strength != null) && (strength[k] > 0)) {
                    stbs.append("{").append("").append(strength[k]).append("}");
                }
            }
        }
        if (stbs.length() == 0)
            return "Pairs: None";
        else
            return "Pairs: " + stbs.toString();


    }


    private String stbSelfsString(byte[] assumedStecker, byte[] strength) {
        String sfS = "";
        for (int k = 0; k < 26; k++) {
            int s = assumedStecker[k];
            if (k < s) {

            } else if (k == s) {
                sfS += "" + Utils.getChar(k);
                if ((strength != null) && (strength[k] > 0))
                    sfS += "{" + strength[k] + "}";
            }
        }

        if (sfS.length() != 0)
            sfS = "Self: " + sfS;

        return sfS;

    }

}
