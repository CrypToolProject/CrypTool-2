package enigma;

public enum MRingScope {
    ALL,
    ALL_STEPPING_INSIDE_MSG,
    ALL_NON_STEPPING,
    ALL_STEPPING_INSIDE_MSG_AND_ONE_NON_STEPPING,
    STEPPING_INSIDE_MSG_WITH_SMALL_IMPACT_AND_ONE_NON_STEPPING,
    ONE_NON_STEPPING;
    public static MRingScope valueOf(int ordinal) {
        for (MRingScope l : MRingScope.values()) {
            if (l.ordinal() == ordinal) {
                return l;
            }
        }
        return null;
    }
}
