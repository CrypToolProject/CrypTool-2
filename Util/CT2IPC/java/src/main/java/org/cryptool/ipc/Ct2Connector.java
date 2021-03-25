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

import java.io.PrintStream;
import java.util.List;
import java.util.Map;
import java.util.concurrent.atomic.AtomicReference;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;
import java.util.logging.Level;
import java.util.logging.Logger;

import org.cryptool.ipc.loops.IReceiveLoop;
import org.cryptool.ipc.loops.ISendLoop;
import org.cryptool.ipc.loops.impl.AbstractLoop.LoopState;
import org.cryptool.ipc.loops.impl.NPHelper;
import org.cryptool.ipc.loops.impl.NamedPipeReceiver;
import org.cryptool.ipc.loops.impl.NamedPipeSender;
import org.cryptool.ipc.messages.Ct2IpcMessages.Ct2IpcMessage;
import org.cryptool.ipc.messages.Ct2IpcMessages.Ct2LogEntry.LogLevel;
import org.cryptool.ipc.messages.Ct2MessageType;
import org.cryptool.ipc.messages.MessageHelper;
import org.cryptool.ipc.messages.TypedMessage;

/**
 * @author Henner Heck
 *
 */
public final class Ct2Connector {

	private static final String pipeRX = "serverToClient";
	private static final String pipeTX = "clientToServer";

	private static final Ct2Connector instance = new Ct2Connector();

	private final AtomicReference<IReceiveLoop<Ct2IpcMessage>> receiveLoop = new AtomicReference<>(null);
	private final AtomicReference<ISendLoop<TypedMessage>> sendLoop = new AtomicReference<>(null);
	private final AtomicReference<Ct2ConnectionState> connState = new AtomicReference<>(null);

	private final Lock myLock = new ReentrantLock();

	private Ct2Connector() {
		// it's a singleton
	}

	/**
	 * @return The state of the send loop.
	 */
	public static LoopState getSenderState() {
		final ISendLoop<TypedMessage> loop = Ct2Connector.instance.sendLoop.get();
		return (loop != null) ? loop.getState() : LoopState.SHUTDOWN;
	}

	/**
	 * @return The state of the receive loop.
	 */
	public static LoopState getReceiverState() {
		final IReceiveLoop<Ct2IpcMessage> loop = Ct2Connector.instance.receiveLoop.get();
		return (loop != null) ? loop.getState() : LoopState.SHUTDOWN;
	}

	private void shutdown_(final boolean clearState) {
		this.myLock.lock();
		try {
			final IReceiveLoop<Ct2IpcMessage> rLoop = this.receiveLoop.get();
			if (rLoop != null) {
				rLoop.stop();
			}
			final ISendLoop<TypedMessage> sLoop = this.sendLoop.get();
			if (sLoop != null) {
				sLoop.stop();
			}
			if (clearState) {
				this.connState.set(null);
			}
		} finally {
			this.myLock.unlock();
		}
	}

	private boolean start_(final TypedMessage hello, final PrintStream anErr) throws Exception {
		this.myLock.lock();
		try {
			// shutdown and clear potential previous connection
			this.shutdown_(true);
			// create state and message loops
			final Ct2ConnectionState connState = new Ct2ConnectionState();
			final NamedPipeSender sender = new NamedPipeSender(
					NPHelper.pipeUrl(Ct2Connector.pipeTX + NPHelper.getPID()), connState, anErr);
			final NamedPipeReceiver receiver = new NamedPipeReceiver(
					NPHelper.pipeUrl(Ct2Connector.pipeRX + NPHelper.getPID()), connState, anErr, sender);
			this.connState.set(connState);
			this.receiveLoop.set(receiver);
			this.sendLoop.set(sender);
			// start message loops
			receiver.start();
			sender.start();
			// send initial message
			sender.offer(hello);
			return true;
		} finally {
			this.myLock.unlock();
		}
	}

	/**
	 *
	 * Shuts down and clears any existing connection and tries to establish a new
	 * connection.
	 *
	 * @param anErr
	 * @return
	 * @throws Exception
	 */
	public static boolean start(final String aProgramName, final String aProgramVersion, final PrintStream anErr)
			throws Exception {
		final TypedMessage hello = //
				MessageHelper.encodeCt2Hello(Ct2MessageType.myProtocolVersion, aProgramName, aProgramVersion);
		return Ct2Connector.instance.start_(hello, anErr);
	}

	/**
	 * Signal the send and the receive loop to stop.
	 */
	public static void stop() {
		Ct2Connector.instance.shutdown_(false);
	}

	private static boolean enqueueWithSender(final TypedMessage m) {
		final ISendLoop<TypedMessage> loop = Ct2Connector.instance.sendLoop.get();
		return (loop != null) ? loop.offer(m) : false;
	}

	/**
	 * @param valuesByPin
	 *            String values with a numerical identifier.
	 * @return True, if the value message was successfully enqueued.
	 */
	public static boolean enqueueValues(final Map<Integer, String> valuesByPin) {
		if ((valuesByPin == null) || valuesByPin.isEmpty()) {
			return true;
		}
		return Ct2Connector.enqueueWithSender(MessageHelper.encodeCt2Values(valuesByPin));
	}

	/**
	 * @param values
	 *            String values.
	 * @return True, if the value message was successfully enqueued.
	 */
	public static boolean enqueueValues(final List<String> values) {
		if ((values == null) || values.isEmpty()) {
			return true;
		}
		return Ct2Connector.enqueueWithSender(MessageHelper.encodeCt2Values(values));
	}

	/**
	 * @param currentValue
	 *            The progress.
	 * @param maxValue
	 *            The maximum progress.
	 * @return True, if the progress message was successfully enqueued.
	 */
	public static boolean enqueueProgress(final double currentValue, final double maxValue) {
		return Ct2Connector.enqueueWithSender(MessageHelper.encodeCt2Progress(currentValue, maxValue));
	}

	/**
	 * @param entry
	 *            The log entry.
	 * @param logLevel
	 *            The log level.
	 * @return True, if the log message was successfully enqueued.
	 */
	public static boolean enqueueLogEntry(final String entry, final LogLevel logLevel) {
		return Ct2Connector.enqueueWithSender(MessageHelper.encodeCt2LogEntry(entry, logLevel));
	}

	/**
	 * @param entry
	 *            The log entry.
	 * @param logLevel
	 *            The log level.
	 * @param localLog
	 *            PrintStream for local log output, possibly System.out or
	 *            System.err
	 * @return True, if the log message was successfully enqueued.
	 */
	public static boolean enqueueLogEntry(final String entry, final LogLevel logLevel, final PrintStream localLog) {
		if (localLog != null) {
			localLog.println(logLevel + ": " + entry);
		}
		return Ct2Connector.enqueueWithSender(MessageHelper.encodeCt2LogEntry(entry, logLevel));
	}

	/**
	 * @param entry
	 *            The log entry.
	 * @param logLevel
	 *            The log level.
	 * @param logger
	 *            An instance of System.Logger.
	 * @return True, if the log message was successfully enqueued.
	 */
	public static boolean enqueueLogEntry(final String entry, final LogLevel logLevel, final Logger logger) {
		if (logger != null) {
			final Level level = MessageHelper.loggerLevel(logLevel);
			if (logger.isLoggable(level)) {
				logger.log(level, entry);
			}
		}
		return Ct2Connector.enqueueWithSender(MessageHelper.encodeCt2LogEntry(entry, logLevel));
	}

	/**
	 * @param exitCode
	 *            The exit code, typically 0 for successful completion.
	 * @param exitMessage
	 *            The exit message.
	 * @return True, if the goodbye message was successfully enqueued.
	 */
	public static boolean enqueueGoodbye(final int exitCode, final String exitMessage) {
		return Ct2Connector.enqueueWithSender(MessageHelper.encodeCt2GoodBye(exitCode, exitMessage));
	}

	// calls to the connection state

	/**
	 * @return The server name.
	 */
	public static String getServerCtName() {
		Ct2ConnectionState cs = Ct2Connector.instance.connState.get();
		return cs != null ? cs.getServerCtName() : "";
	}

	/**
	 * @return The server version.
	 */
	public static String getServerCtVersion() {
		Ct2ConnectionState cs = Ct2Connector.instance.connState.get();
		return cs != null ? cs.getServerCtVersion() : "";
	}

	/**
	 * @return True, if values from the server are available.
	 */
	public static boolean hasValues() {
		Ct2ConnectionState cs = Ct2Connector.instance.connState.get();
		return cs != null ? cs.hasValues() : false;
	}

	/**
	 * @return The oldest value message received from the server, null if no message
	 *         is present in the receive queue.
	 */
	public static Map<Integer, String> getValues() {
		Ct2ConnectionState cs = Ct2Connector.instance.connState.get();
		return cs != null ? cs.getValues() : null;
	}

	/**
	 * @return True, if the server requested a shutdown.
	 */
	public static boolean getShutdownRequested() {
		Ct2ConnectionState cs = Ct2Connector.instance.connState.get();
		return cs != null ? cs.getShutdownRequested() : false;
	}

	/**
	 * @return True, if both sender and receiver loop are in SHUTDOWN state or don't
	 *         exist.
	 */
	public static boolean isShutdown() {
		return (Ct2Connector.getReceiverState() == LoopState.SHUTDOWN) //
				&& (Ct2Connector.getSenderState() == LoopState.SHUTDOWN);
	}

	/**
	 * @param timeoutMillis
	 *            Timeout in milliseconds. Set to 0 or negative number for no
	 *            timeout.
	 * @return True, as soon as both sender and receiver loop are in SHUTDOWN state
	 *         or don't exist. <br>
	 *         False, if timeout is reached.
	 * @throws InterruptedException
	 */
	public static boolean waitForShutdown(final int timeoutMillis) throws InterruptedException {
		final boolean hasTimeout = timeoutMillis > 0;
		final long startMillis = hasTimeout ? System.currentTimeMillis() : 0;
		boolean shutdown = Ct2Connector.isShutdown();
		while (!shutdown) {
			if (hasTimeout && ((System.currentTimeMillis() - startMillis) >= timeoutMillis)) {
				return false;
			}
			Thread.sleep(20);
			shutdown = Ct2Connector.isShutdown();
		}
		return true;
	}

	/**
	 * @return True, if either the sender or the receiver loop has thrown an
	 *         exception.
	 */
	public static boolean hasExceptions() {
		Ct2ConnectionState cs = Ct2Connector.instance.connState.get();
		return cs != null ? ((cs.getSenderException() != null) || (cs.getReceiverException() != null)) : false;
	}

	/**
	 * @return Exception thrown by the sender loop, if available. Null otherwise.
	 *         <br>
	 *         The exception is cleared in the connection state.
	 */
	public static Exception clearSenderException() {
		Ct2ConnectionState cs = Ct2Connector.instance.connState.get();
		return cs != null ? cs.clearSenderException() : null;
	}

	/**
	 * @return Exception thrown by the receiver loop, if available. Null otherwise.
	 *         <br>
	 *         The exception is cleared in the connection state.
	 */
	public static Exception clearReceiverException() {
		Ct2ConnectionState cs = Ct2Connector.instance.connState.get();
		return cs != null ? cs.clearReceiverException() : null;
	}
}
