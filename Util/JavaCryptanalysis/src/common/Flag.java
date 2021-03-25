package common;

public enum Flag {
    INDICATORS_FILE("d"),
    STRENGTH("e"),
    SCENARIO_PATH("f"),
    LANGUAGE("g"),
    HC_SA_CYCLES("h"),
    CIPHERTEXT("i"),
    CRIB_POSITION("j"),
    KEY("k"),
    SIMULATION_TEXT_LENGTH("l"),
    MODEL("m"),
    CYCLES("n"),
    OFFSET("o") /* multiplex*/, SIMULATION_OVERLAPS("o") /* M209 */, MODE("o") /* Enigma*/,
    CRIB("p"),
    RESOURCE_PATH("r"),
    SIMULATION("s"),
    THREADS("t"),
    VERBOSE("u"),
    HELP("v"),
    MESSAGE_INDICATOR("w"),
    VERSION("y") /* M209 */, MIDDLE_RING_SCOPE("y") /* Enigma */,
    RIGHT_RING_SAMPLING("x"), HEXA_FILE("h"),
    SCENARIO("z"),
    ;

    String string;
    private Flag(String string) {
        this.string = string;
    }
    @Override
    public String toString() {
        return string;
    }
}
