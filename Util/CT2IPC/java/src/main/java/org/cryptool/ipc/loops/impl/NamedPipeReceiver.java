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

import java.io.InputStream;
import java.io.PrintStream;
import java.io.RandomAccessFile;

import org.cryptool.ipc.Ct2ConnectionState;
import org.cryptool.ipc.loops.IReceiveLoop;
import org.cryptool.ipc.messages.Ct2IpcMessages.Ct2IpcMessage;
import org.cryptool.ipc.messages.Ct2MessageType;
import org.cryptool.ipc.messages.MessageHelper;

public final class NamedPipeReceiver extends AbstractLoop<Ct2IpcMessage> implements IReceiveLoop<Ct2IpcMessage> {

	private final String pipeUrl;
	private final Ct2ConnectionState connState;
	private final AbstractLoop<?> sendLoop;

	public NamedPipeReceiver(final String aPipeUrl, final Ct2ConnectionState aConnState, final PrintStream anErr,
			final AbstractLoop<?> aSendLoop) {
		super(aConnState, anErr, LoopType.RECEIVER);
		this.pipeUrl = aPipeUrl;
		this.connState = aConnState;
		this.sendLoop = aSendLoop;
	}

	@Override
	public void run() {
		if (this.myState.compareAndSet(LoopState.STARTING, LoopState.RUNNING)) {
			try {
				final RandomAccessFile pipe = NPHelper.connectPipe(this.pipeUrl, "r",
						AbstractLoop.DelayOnConnectionError, AbstractLoop.MaxConnectionErrors, this.myState);
				if (this.myState.get() != LoopState.RUNNING) {
					return;
				}
				if (pipe == null) {
					this.setStopped(false);
					return;
				}
				InputStream is = NPHelper.getInputStream(pipe, AbstractLoop.DelayOnConnectionError,
						AbstractLoop.MaxConnectionErrors);
				// possibly unnecessary optimization to avoid
				// polling the atomic boolean on each pass
				int stateUpdateCounter = 0;
				LoopState state = this.myState.get();
				long loopSleep = 0;
				while (state == LoopState.RUNNING) {
					if (is.available() > 0) {
						// we assume, that there is always a complete message
						final Ct2IpcMessage m = Ct2IpcMessage.parseDelimitedFrom(is);
						final boolean handled = MessageHelper.handleMessage(m, this.connState);
						if (!handled) {
							throw new Exception("Unhandled message with type id " + m.getMessageType() + ".");
						}
						if (Ct2MessageType.SHUTDOWN.getId() == m.getMessageType()) {
							this.setStopped(true);
							return;
						}
						loopSleep = 0;
					} else {
						loopSleep = Math.min(loopSleep + AbstractLoop.LoopSleepIncrement, AbstractLoop.MaxLoopSleep);
					}
					if (++stateUpdateCounter >= AbstractLoop.LoopstateUpdatePeriod) {
						state = this.myState.get();
						stateUpdateCounter = 0;
					}
					if ((loopSleep > 0) && (state == LoopState.RUNNING)) {
						Thread.sleep(loopSleep);
					}
				}
			} catch (Exception e) {
				this.printErr("The message receiver encountered an exception and is shutting down.", e);
			}
			// The message receiver shuts down.
			// The pipe must be closed by cryptool.
			this.setStopped(false);
		}
	}

	private void setStopped(final boolean shutdownRequested) {
		if (shutdownRequested) {
			this.connState.setShutdownRequested();
		}
		if (this.sendLoop != null) {
			this.sendLoop.stop();
		}
		this.myState.set(LoopState.SHUTDOWN);
	}

}
