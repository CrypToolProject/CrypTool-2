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

import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;
import java.util.logging.Level;

import org.cryptool.ipc.Ct2ConnectionState;
import org.cryptool.ipc.messages.Ct2IpcMessages.Ct2Goodbye;
import org.cryptool.ipc.messages.Ct2IpcMessages.Ct2Hello;
import org.cryptool.ipc.messages.Ct2IpcMessages.Ct2IpcMessage;
import org.cryptool.ipc.messages.Ct2IpcMessages.Ct2LogEntry;
import org.cryptool.ipc.messages.Ct2IpcMessages.Ct2LogEntry.LogLevel;
import org.cryptool.ipc.messages.Ct2IpcMessages.Ct2Progress;
import org.cryptool.ipc.messages.Ct2IpcMessages.Ct2Values;

import com.google.protobuf.ByteString;
import com.google.protobuf.InvalidProtocolBufferException;
import com.google.protobuf.ProtocolStringList;

public final class MessageHelper {

	public static boolean handleMessage(Ct2IpcMessage message, Ct2ConnectionState state)
			throws InvalidProtocolBufferException {
		if (message == null) {
			return true;
		}
		final Ct2MessageType type = Ct2MessageType.getTypeForId(message.getMessageType());
		if (type == null) {
			return false;
		}
		final ByteString body = message.getBody();
		switch (type) {
		case HELLO: {
			Ct2Hello m = Ct2Hello.parseFrom(body);
			state.setServerProtocolVersion(m.getProtocolVersion());
			state.setServerCtName(m.getProgramName());
			state.setServerCtVersion(m.getProgramVersion());
		}
			return true;
		case VALUES: {
			Ct2Values m = Ct2Values.parseFrom(body);
			state.addValues(MessageHelper.valueMap(m));
		}
			return true;
		case SHUTDOWN: {
			state.setShutdownRequested();
		}
			return true;
		default:
			return false;
		}
	}

	private static Map<Integer, String> valueMap(Ct2Values v) {
		final int mapSize = Math.min(v.getPinIdCount(), v.getValueCount());
		final Map<Integer, String> valueMap = new TreeMap<>();
		if (mapSize <= 0) {
			return valueMap;
		}
		List<Integer> pinIdList = v.getPinIdList();
		ProtocolStringList valueList = v.getValueList();
		for (int i = 0; i < mapSize; i++) {
			valueMap.put(pinIdList.get(i), valueList.get(i));
		}
		return valueMap;
	}

	public static TypedMessage encodeCt2Hello(final int aProtocolVersion, final String aProgramName,
			final String aProgramVersion) {
		final String pName = (aProgramName != null) && !aProgramName.isEmpty() ? aProgramName : "UNKNOWN";
		final String pVersion = (aProgramVersion != null) && !aProgramVersion.isEmpty() ? aProgramVersion : "UNKNOWN";
		return new TypedMessage(Ct2MessageType.HELLO, //
				Ct2Hello.newBuilder()//
						.setProtocolVersion(aProtocolVersion)//
						.setProgramName(pName)//
						.setProgramVersion(pVersion)//
						.build().toByteString());
	}

	public static TypedMessage encodeCt2Values(final Map<Integer, String> valuesByPinId) {
		final List<Integer> pinIds = new ArrayList<>(valuesByPinId.keySet());
		final List<String> values = new ArrayList<>(valuesByPinId.values());
		return new TypedMessage(Ct2MessageType.VALUES, //
				Ct2Values.newBuilder()//
						.addAllPinId(pinIds)//
						.addAllValue(values)//
						.build().toByteString());
	}

	public static TypedMessage encodeCt2Values(final List<String> valueList) {
		final List<Integer> pinIds = MessageHelper.idList(valueList.size());
		final List<String> values = new ArrayList<>(valueList);
		return new TypedMessage(Ct2MessageType.VALUES, //
				Ct2Values.newBuilder()//
						.addAllPinId(pinIds)//
						.addAllValue(values)//
						.build().toByteString());
	}

	private static List<Integer> idList(final int size) {
		final List<Integer> list = new ArrayList<>(size);
		for (int i = 1; i <= size; i++) {
			list.add(i);
		}
		return list;
	}

	public static TypedMessage encodeCt2Progress(final double currentValue, final double maxValue) {
		return new TypedMessage(Ct2MessageType.PROGRESS, //
				Ct2Progress.newBuilder()//
						.setCurrentValue(currentValue)//
						.setMaxValue(maxValue)//
						.build().toByteString());
	}

	public static TypedMessage encodeCt2LogEntry(final String entry, final LogLevel logLevel) {
		return new TypedMessage(Ct2MessageType.LOGENTRY, //
				Ct2LogEntry.newBuilder()//
						.setEntry(entry)//
						.setLogLevel(logLevel)//
						.build().toByteString());
	}

	public static TypedMessage encodeCt2GoodBye(final int anExitCode, final String anExitMessage) {
		final String message = (anExitMessage != null) && !anExitMessage.isEmpty() ? anExitMessage : "";
		return new TypedMessage(Ct2MessageType.GOODBYE, //
				Ct2Goodbye.newBuilder()//
						.setExitCode(anExitCode)//
						.setExitMessage(message)//
						.build().toByteString());
	}

	public static Level loggerLevel(final LogLevel level) {
		final Level defaultLevel = Level.SEVERE;
		if (level == null) {
			return defaultLevel;
		}
		switch (level) {
		case CT2INFO:
		case CT2BALLOON:
			return Level.INFO;
		case CT2DEBUG:
			return Level.FINE;
		case CT2ERROR:
			return Level.SEVERE;
		case CT2WARNING:
			return Level.WARNING;
		default:
			// This should not happen.
			return defaultLevel;
		}
	}

}
