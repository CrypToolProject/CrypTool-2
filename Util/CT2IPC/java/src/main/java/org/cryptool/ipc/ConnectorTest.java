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

import java.util.HashMap;
import java.util.Map;

import org.cryptool.ipc.messages.Ct2IpcMessages.Ct2LogEntry.LogLevel;

final class ConnectorTest {

	public static void main(String[] args) {
		try {
			System.out.println("Starting Tester...");
			Ct2Connector.start("Tester", "0.1", null);
			System.out.println("Tester started...");

			for (int i = 1; i <= 100; i++) {

				Ct2Connector.enqueueProgress(i, 100);
				Thread.sleep(100);

				// check if ct2 wants us to shutdown
				if (Ct2Connector.getShutdownRequested()) {
					return;
				}

				// test send log
				if ((i != 0) && ((i % 10) == 0)) {
					Ct2Connector.enqueueLogEntry("Hello :-)", LogLevel.CT2INFO);
				}

				// test sending values
				Map<Integer, String> valuemap = new HashMap<Integer, String>();
				valuemap.put(1, "" + i);
				valuemap.put(2, "" + (i + 1));
				valuemap.put(3, "" + (i + 2));
				Ct2Connector.enqueueValues(valuemap);

				// test receiving values
				if (Ct2Connector.hasValues()) {
					Map<Integer, String> values = Ct2Connector.getValues();

					if (values.containsKey(1)) {
						System.out.println("Received value for 1:" + values.get(1));
					}
					if (values.containsKey(2)) {
						System.out.println("Received value for 2:" + values.get(2));
					}
					if (values.containsKey(3)) {
						System.out.println("Received value for 3:" + values.get(3));
					}
				}
			}
			try {
				Ct2Connector.enqueueGoodbye(0, "everything ok!");
			} catch (Exception e) {
				e.printStackTrace();
			}
		} catch (Exception e) {
			e.printStackTrace();
			try {
				Ct2Connector.enqueueGoodbye(-1, e.getMessage());
			} catch (Exception e2) {
				e2.printStackTrace();
			}
		}
	}
}
