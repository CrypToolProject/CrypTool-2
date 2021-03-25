package m209;

public class Global {

    public static Version VERSION = initVersion(Version.V1947);

    public static boolean FROM_TABLE;
    public static boolean ONLY_TABLE_GROUP_A;
    public static boolean ONLY_TABLE_GROUP_B;

    public static int MAX_OVERLAP;
    public static int MIN_OVERLAP;
    public static int MAX_KICK;
    public static int MAX_KICK_REPETITION_64;
    public static boolean COVERAGE_27;
    public static boolean SAME_SUCCESSOR_ALLOWED;
    public static boolean EVEN_3;
    public static boolean EVEN_2_3_4;
    public static int MAX_CONSECUTIVE_SAME_PINS;
    public static int MAX_SAME_OVERLAP;
    public static int MIN_INVOLVED_WHEELS;
    public static boolean OVERLAPS_SIDEBYSIDE_SEPARATED;
    public static boolean OVERLAPS_EVENLY;
    public static int MAX_TOTAL_OVERLAP;
    public static int MAX_LOWEST_KICK;
    public static int MIN_KICK;
    public static int MIN_PERCENT_ACTIVE_PINS;
    public static int MAX_PERCENT_ACTIVE_PINS;

    public static Version initVersion(Version version) {
        return initVersion(version, false, false);
    }

    private static Version initVersion(Version version, boolean onlyA, boolean onlyB) {
        VERSION = version;

        if ((VERSION == Version.V1942) || (VERSION == Version.V1943)|| (VERSION == Version.V1944)|| (VERSION == Version.V1947)|| (VERSION == Version.V1953)) {
            ONLY_TABLE_GROUP_A = onlyA;
            ONLY_TABLE_GROUP_B = onlyB;
            FROM_TABLE = (VERSION == Version.V1944) || (VERSION == Version.V1947);
            MAX_OVERLAP = 12;
            MIN_OVERLAP = ((VERSION == Version.V1943) || (VERSION == Version.V1944)) ? 1 : 2;
            MIN_KICK = 1;
            MAX_KICK = (VERSION == Version.V1953) ? 14 : 13;
            MAX_KICK_REPETITION_64 = (VERSION == Version.V1953) ? 5 : Integer.MAX_VALUE;
            MAX_LOWEST_KICK = 1;
            SAME_SUCCESSOR_ALLOWED = VERSION != Version.V1942;
            EVEN_3 = VERSION == Version.V1942;
            EVEN_2_3_4 = VERSION == Version.V1943;
            MAX_CONSECUTIVE_SAME_PINS = (VERSION == Version.V1944) || (VERSION == Version.V1947) || (VERSION == Version.SWEDISH) ? 6 : Integer.MAX_VALUE;
            MAX_SAME_OVERLAP = ((VERSION == Version.V1942) || (VERSION == Version.V1943)) ? Integer.MAX_VALUE : 4;
            MIN_INVOLVED_WHEELS = (VERSION != Version.V1942) ? 4 : 0;
            OVERLAPS_SIDEBYSIDE_SEPARATED = (VERSION != Version.V1942);
            OVERLAPS_EVENLY = (VERSION != Version.V1942);
            MAX_TOTAL_OVERLAP = ((VERSION == Version.V1947) || (VERSION == Version.V1953)) ? 1 : Integer.MAX_VALUE;
            COVERAGE_27 = true;
            MAX_PERCENT_ACTIVE_PINS = 70;
            MIN_PERCENT_ACTIVE_PINS = 30;

        } else if (version == Version.SWEDISH) {
            FROM_TABLE = false;
            MAX_OVERLAP = 15;
            MIN_OVERLAP = 0;
            MAX_KICK = Key.BARS;
            MAX_KICK_REPETITION_64 = 100;
            SAME_SUCCESSOR_ALLOWED = true;
            EVEN_3 = false;
            EVEN_2_3_4 = false;

            MAX_SAME_OVERLAP = 100;
            MIN_INVOLVED_WHEELS = 0;
            OVERLAPS_SIDEBYSIDE_SEPARATED = false;
            OVERLAPS_EVENLY = false;
            MAX_TOTAL_OVERLAP = Key.BARS;
            MIN_KICK = 0;
            MAX_LOWEST_KICK = MAX_KICK;
            COVERAGE_27 = false;
        } else if (version == Version.UNRESTRICTED) {
            FROM_TABLE = false;
            MAX_OVERLAP = Key.BARS;
            MIN_OVERLAP = 0;
            MAX_KICK = Key.BARS;
            MAX_KICK_REPETITION_64 = 100;
            SAME_SUCCESSOR_ALLOWED = true;
            EVEN_3 = false;
            EVEN_2_3_4 = false;
            MAX_CONSECUTIVE_SAME_PINS = 100;
            MAX_PERCENT_ACTIVE_PINS = 100;
            MIN_PERCENT_ACTIVE_PINS = 0;
            MAX_SAME_OVERLAP = 100;
            MIN_INVOLVED_WHEELS = 0;
            OVERLAPS_SIDEBYSIDE_SEPARATED = false;
            OVERLAPS_EVENLY = false;
            MAX_TOTAL_OVERLAP = Key.BARS;
            MIN_KICK = 0;
            MAX_LOWEST_KICK = MAX_KICK;
            COVERAGE_27 = false;
        } else if (version == Version.NO_OVERLAP) {
            FROM_TABLE = false;
            MAX_OVERLAP = 0;
            MIN_OVERLAP = 0;
            MAX_KICK = Key.BARS;
            MAX_KICK_REPETITION_64 = 100;
            SAME_SUCCESSOR_ALLOWED = true;
            EVEN_3 = false;
            EVEN_2_3_4 = false;
            MAX_CONSECUTIVE_SAME_PINS = 6;
            MAX_PERCENT_ACTIVE_PINS = 80;
            MIN_PERCENT_ACTIVE_PINS = 20;
            MAX_SAME_OVERLAP = 100;
            MIN_INVOLVED_WHEELS = 0;
            OVERLAPS_SIDEBYSIDE_SEPARATED = false;
            OVERLAPS_EVENLY = false;
            MAX_TOTAL_OVERLAP = 0;
            MIN_KICK = 0;
            MAX_LOWEST_KICK = MAX_KICK;
            COVERAGE_27 = true;
        }

        LugsRules.createValidLugCountSequences();

        return VERSION;

    }

}
