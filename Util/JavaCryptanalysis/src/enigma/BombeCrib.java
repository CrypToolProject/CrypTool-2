package enigma;

class BombeCrib {
    public static final int MAXCRIBL = 100;
    // todo - add setters and getters
    private final byte[] crib = new byte[MAXCRIBL];
    public static final double BADSCORE = 1000.0;
    private final int cribLen;
    private final int cribPos; // position of the crib in the cipher text
    public BombeMenu menu;

    BombeCrib(byte[] ciphertext, byte[] cr, int crl, int cpos, boolean print) {

        cribPos = cpos;
        System.arraycopy(cr, 0, crib, 0, crl);
        cribLen = crl;

        int links[][] = new int[26][26];  // positions in which letters i and j correspond to input/output of scramblers (earliest position)

        if (print) {
            for (int i = 0; i < cribLen; i++)
                System.out.printf("%d", ((i + cpos) / 10) % 10);
            System.out.print("\n");
            for (int i = 0; i < cribLen; i++)
                System.out.printf("%d", (i + cpos) % 10);
            System.out.print("\n");
            for (int i = 0; i < cribLen; i++)
                System.out.printf("%s", Utils.getChar(ciphertext[i + cpos]));
            System.out.print("\n");
            for (int i = 0; i < cribLen; i++)
                System.out.printf("%s", Utils.getChar(crib[i]));
            System.out.print("\n");
            for (int i = 0; i < cribLen; i++)
                System.out.printf("%d", i % 10);
            System.out.print("\n\n");
        }
        for (int i = 0; i < 26; i++) {
            for (int j = 0; j < 26; j++)
                links[i][j] = -1;
        }

        for (int j = 0; j < cribLen; j++) {
            int ctletter = ciphertext[cpos + j];
            int crletter = crib[j];
            if (crletter != -1)
                if (links[ctletter][crletter] == -1) {
                    links[ctletter][crletter] = cpos + j;
                    //System.out.printf("%s->%s at %d\n", Utils.getChar(ctletter),Utils.getChar(crletter),links[ctletter][crletter]);

                }
        }        
/*
       for (int i = 0; i < 26 ; i++) {
            for (int j = 0; j < 26; j++) 
                if (links[i][j]!=-1)
                    System.out.printf("%s->%s (%d)", Utils.getChar(i),Utils.getChar(j),links[i][j]);
            
       } 
       System.out.printf("END OF LINKS FOR POS %d \n",pos);
*/
        createMenu(links, print);

    }

    // public static utilities
    public static double score(int closures, int count) {

        Double score;


        switch (count) {
            case 0:
                score = 10000000.0;
                break;
            case 1:
                score = 1600000.0;
                break;
            case 2:
                score = 800000.0;
                break;
            case 3:
                score = 450000.0;
                break;
            case 4:
                score = 300000.0;
                break;
            case 5:
                score = 180000.0;
                break;
            case 6:
                score = 120000.0;
                break;
            case 7:
                score = 70000.0;
                break;
            case 8:
                score = 40000.0;
                break;
            case 9:
                score = 19000.0;
                break;
            case 10:
                score = 7300.0;
                break;
            case 11:
                score = 2700.0;
                break;
            case 12:
                score = 820.0;
                break;
            case 13:
                score = 200.0;
                break;
            case 14:
                score = 43.0;
                break;
            case 15:
                score = 7.3;
                break;
            case 16:
                score = 1.0;
                break;
            case 17:
                score = 0.125;
                break;
            case 18:
                score = 0.015;
                break;

            default:
                score = 0.001;
                break;
        }
        for (int i = 0; i < closures; i++)
            score /= 26.0;

        if (score < 0.001)
            score = 0.001;
        return score;


    }

    public static int nextValidPosition(byte[] ct, int ctl, byte[] cr, int crl, int pos) {

        int validPos = -1;

        for (int i = pos; (i <= (ctl - crl)) && (validPos == -1); i++) {
            validPos = i;
            for (int j = 0; j < crl; j++)
                if (ct[i + j] == cr[j]) {
                    validPos = -1;
                    break;
                }

        }

        return validPos;

    }

    // Create and populate a menu and its subgraphs
    private void createMenu(int[][] links, boolean print) {


        if (print) {
            System.out.printf("Creating menu subgraphs - Links for crib at position %d: ", cribPos);

            for (int i = 0; i < 26; i++) {
                for (int j = 0; j < 26; j++)
                    if (links[i][j] != -1)
                        System.out.printf("%s->%s at %d, ", Utils.getChar(i), Utils.getChar(j), links[i][j]);

            }
            System.out.print("\n\n");


        }

        menu = new BombeMenu(cribPos, cribLen, crib);

        // Count how many times letter has been traverse, for the whole graph.
        int graphLetterUsage[] = new int[26];
        for (int letter = 0; letter < 26; letter++)
            graphLetterUsage[letter] = 0;

        for (int letter = 0; letter < 26; letter++) {
            // Start subgraph traversal with unused letter.
            if (graphLetterUsage[letter] > 0)
                continue;

            // Count how many times letter has been traversed, for the subgraph.
            int subgraphLetterUsage[] = new int[26];
            for (int k = 0; k < 26; k++)
                subgraphLetterUsage[k] = 0;

            traverseSubgraph(links, 0, "", letter, subgraphLetterUsage, -1);

            int subgraphLetterCount = 0;
            int subgraphClosures = 0;
            for (int k = 0; k < 26; k++) {
                if (subgraphLetterUsage[k] > 0) {
                    graphLetterUsage[k] += subgraphLetterUsage[k];
                    subgraphLetterCount++;
                    if (subgraphLetterUsage[k] > 1)
                        subgraphClosures += (subgraphLetterUsage[k] - 1);
                }
            }
            subgraphClosures /= 2; //they were counted twice ....

            // A subgraph should have at least 2 letters.
            if (subgraphLetterCount < 2)
                continue;

            menu.addSubgraph(links, subgraphClosures, subgraphLetterUsage, print);


        } // for letter ...

        // Compute the Turing score for the whole graph.
        menu.score = score(menu.totalClosures, menu.totalItems);

        if (print)
            System.out.printf("Total %d subgraphs found, total %d closures and %d links, Turing score: %.2f \n\n",
                    menu.nSubgraphs, menu.totalClosures, menu.totalItems, menu.score);

        // Sort the subgraphs by score. When running the bombe to test a certain settings, we will start checking the
        // subgraph with the highest score.
        menu.sortSubgraphs(print);

    }

    // Count the usages of the letters in a subgraph starting from a given letter.
    // stopPosition is specified to avoid endless loops in recursive call. If we get back to that position, no need to continue.
    private void traverseSubgraph(int[][] links, int level, String prefix, int startLetter, int[] letterUsage, int stopPositiom) {

        // Level and prefix used for debug only.
        prefix += Utils.getChar(startLetter);
    
        /*
        String freeS = "";
        for (int i = 0; i < 26; i++)
            if (letterUsage[i]== 0)
                freeS+=Utils.getChar(i);
        System.out.printf("COUNT BEFORE = LEVEL %d PREF = %s LETTER=%s (%d) FREE = %s\n", 
                level,  prefix, Utils.getChar(letter), letterUsage[letter], freeS);
        */
        //Count how many times a letter has been used (traversed) - if twice, then we have closure.
        letterUsage[startLetter]++;
        if (letterUsage[startLetter] > 1)
            return;

        for (int nextLetter = 0; nextLetter < 26; nextLetter++) {
            // Look at links from an to the start letter.
            int pos1 = links[startLetter][nextLetter];
            int pos2 = links[nextLetter][startLetter];

            if ((pos1 != -1) && (pos1 != stopPositiom))
                traverseSubgraph(links, level + 1, prefix, nextLetter, letterUsage, pos1);
            else if ((pos2 != -1) && (pos2 != stopPositiom))
                traverseSubgraph(links, level + 1, prefix, nextLetter, letterUsage, pos2);


        }    
   /*
        freeS = "";
        for (int i = 0; i < 26; i++)
            if (letterUsage[i]== 0)
                freeS+=Utils.getChar(i);
        System.out.printf("COUNT AFTER  = LEVEL %d PREF = %s LETTER=%s (%d)  FREE = %s\n", level,  prefix, Utils.getChar(letter), letterUsage[letter],freeS);
   */
    }


}
