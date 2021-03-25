import logging
from logging import INFO

class InterceptLogHandler:
    def __init__(self, handler):
        self._handler = handler
    
    def flush(self):
        pass

    def write(self, msg):
        self._handler(msg)

class HandlerFormatter(logging.Formatter):
    formatstr = \
       '%(process)s|%(created)s|%(levelnametitle)s|%(name)s|%(message)s' 
    
    def format(self, record):
        record.levelnametitle = record.levelname.title()
        return super().format(record)
    
    def __init__(self):
        super().__init__(fmt=self.formatstr)

def registerLogger(logHandler):
    logger = logging.getLogger()
    logHandler = logging.StreamHandler(InterceptLogHandler(logHandler))
    logHandler.setLevel(logging.INFO)
    logHandler.setFormatter(HandlerFormatter())
    logger.addHandler(logHandler)
