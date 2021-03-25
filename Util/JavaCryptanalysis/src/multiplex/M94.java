package multiplex;

import common.Utils;

class M94 extends Multiplex{
    private static String[] STRIPS = {"ABCEIGDJFVUYMHTQKZOLRXSPWN",
            "ACDEHFIJKTLMOUVYGZNPQXRWSB",
            "ADKOMJUBGEPHSCZINXFYQRTVWL",
            "AEDCBIFGJHLKMRUOQVPTNWYXZS",
            "AFNQUKDOPITJBRHCYSLWEMZVXG",
            "AGPOCIXLURNDYZHWBJSQFKVMET",
            "AHXJEZBNIKPVROGSYDULCFMQTW",
            "AIHPJOBWKCVFZLQERYNSUMGTDX",
            "AJDSKQOIVTZEFHGYUNLPMBXWCR",
            "AKELBDFJGHONMTPRQSVZUXYWIC",
            "ALTMSXVQPNOHUWDIZYCGKRFBEJ",
            "AMNFLHQGCUJTBYPZKXISRDVEWO",
            "ANCJILDHBMKGXUZTSWQYVORPFE",
            "AODWPKJVIUQHZCTXBLEGNYRSMF",
            "APBVHIYKSGUENTCXOWFQDRLJZM",
            "AQJNUBTGIMWZRVLXCSHDEOKFPY",
            "ARMYOFTHEUSZJXDPCWGQIBKLNV",
            "ASDMCNEQBOZPLGVJRKYTFUIWXH",
            "ATOJYLFXNGWHVCMIRBSEKUPDZQ",
            "AUTRZXQLYIOVBPESNHJWMDGFCK",
            "AVNKHRGOXEYBFSJMUDQCLZWTIP",
            "AWVSFDLIEBHKNRJQZGMXPUCOTY",
            "AXKWREVDTUFOYHMLSIQNJCPGBZ",
            "AYJPXMVKBQWUGLOSTECHNZFRID",
            "AZDNBUHYFWJLVGRCQMPSOEXTKI"
    };

    int[] offsets;


    M94(int numberOfOffsets) {
        super(STRIPS, 25);
        this.offsets = new int[numberOfOffsets];
    }
    M94(int[] offsets) {
        this(offsets.length);
        System.arraycopy(offsets, 0, this.offsets, 0, offsets.length);
    }
    M94(int[] key, int[] offsets) {
        this(offsets);
        setKey(key);
    }

    M94 randomizeOffsets() {
        for (int i = 0; i < offsets.length; i++) {
            setOffset(i, 1 + Utils.randomNextInt(STRIP_LENGTH - 1));
        }
        decryptionValid = false;
        return this;
    }

    M94 setOffset(int index, int offset) {
        this.offsets[index] = offset;
        this.decryptionValid = false;
        return this;
    }

    String offsetString(int o) {
        return String.format("%02d", offsets[o]);
    }

    @Override
    String offsetString() {
        StringBuilder str = new StringBuilder();
        for (int o = 0; o < offsets.length; o++) {
            str.append((o == 0) ? "" : ",");
            str.append(offsetString(o));
        }
        return str.toString();
    }

    @Override
    int offset(int i) {
        return offsets[i / NUMBER_OF_STRIPS_USED_IN_KEY];
    }

}
