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
package org.cryptool.ipc.messages;

import java.util.HashMap;
import java.util.Map;

public enum Ct2MessageType {

	HELLO(1), //
	SHUTDOWN(2), //
	VALUES(3), //
	LOGENTRY(4), //
	PROGRESS(5), //
	GOODBYE(6);

	private final int id;

	private Ct2MessageType(final int id) {
		this.id = id;
	}

	public int getId() {
		return this.id;
	}

	private static Map<Integer, Ct2MessageType> makeMap() {
		Map<Integer, Ct2MessageType> map = new HashMap<>(32);
		for (Ct2MessageType t : Ct2MessageType.values()) {
			if (map.containsKey(t.getId())) {
				throw new RuntimeException(Ct2MessageType.class.getSimpleName()
						+ ": Message ids must be unique, but are not with id " + t.getId() + ".");
			}
			map.put(t.getId(), t);
		}
		return map;
	}

	private static final Map<Integer, Ct2MessageType> typesById = makeMap();

	public static Ct2MessageType getTypeForId(final int id) {
		return typesById.get(id);
	}

	// the highest supported protocol version

	public static final int myProtocolVersion = 1;

}
