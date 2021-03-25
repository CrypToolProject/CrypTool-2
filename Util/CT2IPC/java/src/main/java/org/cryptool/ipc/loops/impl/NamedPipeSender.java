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

import java.io.OutputStream;
import java.io.PrintStream;
import java.io.RandomAccessFile;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.TimeUnit;

import org.cryptool.ipc.Ct2ConnectionState;
import org.cryptool.ipc.loops.ISendLoop;
import org.cryptool.ipc.messages.Ct2IpcMessages.Ct2IpcMessage;
import org.cryptool.ipc.messages.TypedMessage;

public final class NamedPipeSender extends AbstractLoop<TypedMessage> implements ISendLoop<TypedMessage> {

	private final BlockingQueue<TypedMessage> queue = new LinkedBlockingQueue<>();
	private final String pipeUrl;

	public NamedPipeSender(final String aPipeUrl, final Ct2ConnectionState aConnState, final PrintStream anErr) {
		super(aConnState, anErr, LoopType.SENDER);
		this.pipeUrl = aPipeUrl;
	}

	@Override
	public boolean offer(TypedMessage message) {
		return this.queue.offer(message);
	}

	@Override
	public void run() {
		if (this.myState.compareAndSet(LoopState.STARTING, LoopState.RUNNING)) {
			long sequenceNumber = 0L;
			try {
				final RandomAccessFile pipe = NPHelper.connectPipe(this.pipeUrl, "rws",
						AbstractLoop.DelayOnConnectionError, AbstractLoop.MaxConnectionErrors, this.myState);
				if (this.myState.get() != LoopState.RUNNING) {
					return;
				}
				final OutputStream os = NPHelper.getOutputStream(pipe, AbstractLoop.DelayOnConnectionError,
						AbstractLoop.MaxConnectionErrors);
				// possibly unnecessary optimization to avoid
				// polling the atomic boolean on each loop
				int stateUpdateCounter = 0;
				LoopState state = this.myState.get();
				while (state == LoopState.RUNNING) {
					final TypedMessage m = this.queue.poll(AbstractLoop.MaxLoopSleep, TimeUnit.MILLISECONDS);
					if (m != null) {
						Ct2IpcMessage.newBuilder()//
								.setSequenceNumber(++sequenceNumber)//
								.setMessageType(m.getType().getId())//
								.setBody(m.getEncodedMessage())//
								.build()//
								.writeDelimitedTo(os);
					}
					if (++stateUpdateCounter >= AbstractLoop.LoopstateUpdatePeriod) {
						state = this.myState.get();
						stateUpdateCounter = 0;
					}
				}
			} catch (Exception e) {
				this.printErr("The message sender encountered an exception and is shutting down.", e);
			}
			// The message receiver shuts down.
			// The pipe must be closed by cryptool.
			this.setStopped();
		}
	}

	private void setStopped() {
		this.myState.set(LoopState.SHUTDOWN);
	}

}
