package common;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.ObjectInputStream;
import java.nio.ByteBuffer;
import java.nio.LongBuffer;
import java.util.HashMap;
import java.util.Map;

public class NGrams {
    private static final Map<Long, Long> map7 = new HashMap<>();
    private static final Map<Long, Long> map8 = new HashMap<>();
    private static final long MASK7 = (long) Math.pow(26, 6);

    private static final boolean[] FILTER = new boolean[(int) Math.pow(26, 6)];
    private static final long MASK8 = (long) Math.pow(26, 7);



    public static long eval7(int[] text, int len) {
        Stats.evaluations++;
        long idx = 0;
        long score = 0;
        for (int i = 0; i < len; i++) {
            idx = (idx % MASK7) * 26 + text[i];
            if (i < 7 - 1) {
                continue;
            }
            if (!FILTER[(int) (idx / 26)]) {
                continue;
            }
            Long v = map7.get(idx);
            if (v == null) {
                continue;
            }
            score += 400_000 * v;
        }

        return score / (len - 7 + 1);
    }

    public static long eval8(int[] text, int len) {
        Stats.evaluations++;
        long idx = 0;
        long score = 0;
        for (int i = 0; i < len; i++) {
            idx = (idx % MASK8) * 26 + text[i];
            if (i < 8 - 1) {
                continue;
            }
            if (!FILTER[(int) (idx / (26 * 26))]) {
                continue;
            }
            Long v = map8.get(idx);
            if (v == null) {
                continue;
            }
            score += 400_000 * v;
        }
        return score / (len - 8 + 1);
    }

    public static boolean load(String statsFilename, int ngrams) {
        try {
            FileInputStream is = new FileInputStream(new File(statsFilename));
            Map<Long, Long> map = ngrams == 8 ? map8 : map7;
            map.clear();


            ObjectInputStream inputStream = new ObjectInputStream(is);
            long[] data = (long[]) inputStream.readObject();
            System.out.printf("Read %,d items from %s\n", data.length/2, statsFilename);
            long using = Math.min(data.length / 2, 1_000_000);
            for (int i = 0; i < using; i++) {
                long index = data[2 * i];
                long value = data[2 * i + 1] + 1;
                map.put(index, (long) (Math.log(value) / Math.log(2)));
                if (ngrams == 7) {
                    FILTER[(int) (index / 26)] = true;
                } else {
                    FILTER[(int) (index / (26 * 26))] = true;
                }
            }
            System.out.printf("Using %,d items from %s (free bytes %,d)\n", using, statsFilename, Runtime.getRuntime().freeMemory());

            is.close();


            return true;
        } catch (IOException | ClassNotFoundException e) {
            e.printStackTrace();
        }
        return false;
    }


}
