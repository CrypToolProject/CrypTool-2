package common;

import common.CtAPI;

import java.util.ArrayList;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;

public class Runnables {

    private ArrayList<Runnable> runnables = new ArrayList<Runnable>();

    public void addRunnable(Runnable runnable){
        runnables.add(runnable);
    }

    public void run(int threads) {

        long start = System.currentTimeMillis();

        ExecutorService threadExecutor = Executors.newFixedThreadPool(threads);
        for (Runnable runnable : runnables) {
            threadExecutor.execute(runnable);
        }

        threadExecutor.shutdown();

        boolean finished = false;
        while (!finished) {
            try {
                finished = threadExecutor.awaitTermination(1, TimeUnit.MICROSECONDS);
            } catch (InterruptedException ex) {
                Thread.currentThread().interrupt();
            }
        }
        long end = System.currentTimeMillis();
        //CtAPI.printf("Elapsed %10d millis for %6d tasks on %d threads\n", end - start, runnables.size(), threads);

    }
}
