namespace M209AnalyzerLib.Enums
{
    public enum Flag
    {
        // https://stackoverflow.com/questions/8588384/how-to-define-an-enum-with-string-value
        INDICATORS_FILE = 'd',
        STRENGTH = 'e',
        SCENARIO_PATH = 'f',
        LANGUAGE = 'g',
        HC_SA_CYCLES = 'h', HEXA_FILE = 'h',
        CIPHERTEXT = 'i',
        CRIB_POSITION = 'j',
        KEY = 'k',
        SIMULATION_TEXT_LENGTH = 'l',
        MODEL = 'm',
        CYCLES = 'n',
        OFFSET = 'o' /* multiplex*/, SIMULATION_OVERLAPS = 'o' /* M209 */, MODE = 'o' /* Enigma*/,
        CRIB = 'p',
        RESOURCE_PATH = 'r',
        SIMULATION = 's',
        THREADS = 't',
        VERBOSE = 'u',
        HELP = 'v',
        MESSAGE_INDICATOR = 'w',
        VERSION = 'y' /* M209 */, MIDDLE_RING_SCOPE = 'y' /* Enigma */,
        RIGHT_RING_SAMPLING = 'x',
        SCENARIO = 'z'
    }
}
