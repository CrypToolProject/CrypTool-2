package enigma;

//hill climbing transformations
enum SearchAction {
    NO_CHANGE,
    IandK,
    IandSK,
    KandSI,
    IandSI,
    KandSK,
    IandK_SIandSK,
    IandSK_KandSI,
    SIandSK,
    RESWAP1,
    RESWAP2
}
