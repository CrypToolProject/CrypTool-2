/*
   Copyright 2018 Henner Heck

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
package org.cryptool.ipc.loops.impl;

import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.io.RandomAccessFile;
import java.util.concurrent.atomic.AtomicReference;

import org.cryptool.ipc.loops.impl.AbstractLoop.LoopState;

public final class NPHelper {

	/**
	 *
	 *
	 * @return The native process id of the process.
	 *
	 */
	public static long getPID() throws Exception {
		final String name = java.lang.management.ManagementFactory.getRuntimeMXBean().getName();
		try {
			return Long.parseLong(name.split("@")[0]);
		} catch (Exception e) {
			throw new Exception("Failed to determine the process id of this process.", e);
		}
	}

	public static String pipeUrl(final String pipename) {
		return "\\\\.\\pipe\\" + pipename;
	}

	public static RandomAccessFile connectPipe(final String pipeUrl, final String rw, final long waitMillis,
			final int tries, final AtomicReference<LoopState> state) throws Exception {
		int tryCounter = 0;
		while (tryCounter < Math.abs(tries)) {
			try {
				RandomAccessFile pipe = new RandomAccessFile(pipeUrl, rw);
				return pipe;
			} catch (FileNotFoundException e) {
				if (state.get() != LoopState.RUNNING) {
					return null;
				}
				tryCounter++;
				Thread.sleep(waitMillis);
			}
		}
		throw new Exception("Failed to connect with pipe \"" + pipeUrl + "\" in mode \"" + rw + "\".");
	}

	public static InputStream getInputStream(final RandomAccessFile pipe, final long waitMillis, final int tries)
			throws Exception {
		int tryCounter = 0;
		while (tryCounter < Math.abs(tries)) {
			try {
				InputStream is = new FileInputStream(pipe.getFD());
				return is;
			} catch (IOException e) {
				tryCounter++;
				Thread.sleep(waitMillis);
			}
		}
		throw new Exception("Failed to get an input stream for the pipe.");
	}

	public static OutputStream getOutputStream(final RandomAccessFile pipe, final long waitMillis, final int tries)
			throws Exception {
		int tryCounter = 0;
		while (tryCounter < Math.abs(tries)) {
			try {
				OutputStream os = new FileOutputStream(pipe.getFD());
				return os;
			} catch (IOException e) {
				tryCounter++;
				Thread.sleep(waitMillis);
			}
		}
		throw new Exception("Failed to get an output stream for the pipe.");
	}

}
