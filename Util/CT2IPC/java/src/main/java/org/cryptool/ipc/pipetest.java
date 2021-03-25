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
package org.cryptool.ipc;

import org.cryptool.ipc.loops.impl.NPHelper;

final class pipetest {

	private static final String pipename = "testpipe";

	public static void main(String[] args) {

		try {
			System.out.println("Process id: " + NPHelper.getPID());
		} catch (Exception e1) {
			// TODO Auto-generated catch block
			e1.printStackTrace();
		}

		// try (RandomAccessFile pipe = new RandomAccessFile(pipePath(pipename), "rw");
		// //
		// FileInputStream fis = new FileInputStream(pipe.getFD()); //
		// FileOutputStream fos = new FileOutputStream(pipe.getFD())) {
		//
		// // Connect to the pipe
		// // String echoText = "Hello world\n Next line!\n";
		// // write to pipe
		// // pipe.write(echoText.getBytes());
		// // read response
		// // String echoResponse = pipe.readLine();
		// // System.out.println("Response: " + echoResponse);
		//
		// Version ver = Version.newBuilder()//
		// .setName("CrypTool") //
		// .setMajor(2)//
		// .setMinor(7)//
		// .build();
		//
		// System.out.println("Sending: " + ver.toString());
		//
		// ver.writeDelimitedTo(fos);
		//
		// Version verParsed = Version.parseDelimitedFrom(fis);
		//
		// System.out.println("Received: " + verParsed.toString());
		//
		// Thread.sleep(3000);
		//
		// } catch (Exception e) {
		// // TODO Auto-generated catch block
		// e.printStackTrace();
		// }

	}

	private static String pipePath(final String pipename) {
		return "\\\\.\\pipe\\" + pipename;
	}

}
