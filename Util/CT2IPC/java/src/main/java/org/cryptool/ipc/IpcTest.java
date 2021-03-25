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

import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;

import org.cryptool.ipc.loops.impl.AbstractLoop.LoopState;

final class IpcTest {

	public static void main(String[] args) {

		// try {
		// Thread.sleep(1000);
		// } catch (InterruptedException e) {
		// // TODO Auto-generated catch block
		// e.printStackTrace();
		// System.exit(1);
		// }

		try {
			Ct2Connector.start("IpcTest", "v0.1", null);
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			System.exit(1);
		}

		for (int i = 0; i < 5; i++) {
			LoopState senderState = Ct2Connector.getSenderState();
			System.out.println("Receiver: " + Ct2Connector.getReceiverState());
			System.out.println("Sender: " + senderState);

			System.out.println("Servername: " + Ct2Connector.getServerCtName());
			System.out.println("Serverversion: " + Ct2Connector.getServerCtVersion());
			System.out.println("Values:");
			IpcTest.printValues(Ct2Connector.getValues());
			System.out.println();
			try {
				Thread.sleep(500);
			} catch (InterruptedException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
				System.exit(1);
			}
		}
		{
			final List<String> values = new ArrayList<>();
			values.add("I have finished.");
			values.add("All done.");
			values.add("Really.");
			Ct2Connector.enqueueValues(values);
		}

		while (Ct2Connector.getSenderState() == LoopState.RUNNING) {
			System.out.println("Waiting for shutdown.");
			try {
				Thread.sleep(500);
			} catch (InterruptedException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
				System.exit(1);
			}
		}
		System.exit(0);
	}

	private static void printValues(Map<Integer, String> map) {
		if (map == null) {
			return;
		}
		for (Entry<Integer, String> entry : map.entrySet()) {
			System.err.println(entry.getKey() + ": " + entry.getValue());
		}
	}
}
