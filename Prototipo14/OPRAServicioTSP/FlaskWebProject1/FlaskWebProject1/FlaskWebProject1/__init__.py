"""
The flask application package.
"""
from logging.config import dictConfig
from flask import Flask, logging
from flask_cors import CORS
import logging
import traceback
from logging.handlers import SMTPHandler
from flask.logging import default_handler
from logging.handlers import RotatingFileHandler
import sentry_sdk
from sentry_sdk.integrations.flask import FlaskIntegration
import sys

dictConfig({
    'version': 1,
    'formatters': {
        'default': {
            'format': '[%(asctime)s] %(levelname)s in %(module)s: %(message)s',
        }
    },
    'handlers': {
        'wsgi': {
            'class': 'logging.StreamHandler',
            'stream': 'ext://flask.logging.wsgi_errors_stream',
            'formatter': 'default'
        },
        'file.handler': {
            'class': 'logging.handlers.RotatingFileHandler',
            'filename': './logs/test.log',
            'maxBytes': 4194304,  # 4 MB
            'backupCount': 10, 
            'level': 'WARNING',
            'formatter': 'default',
        }
    },
    'loggers': {
        'werkzeug': {
            'handlers': ['file.handler'],
        },
    },
    'root': {
        'level': 'INFO',
        'handlers': ['wsgi','file.handler'],
    }
})
mail_handler = SMTPHandler(
    mailhost=('smtp.iboof.com', 25),
    fromaddr='servidor@iboof.com',
    toaddrs=['iperez@itelligent.es'],
    credentials=('servidor@iboof.com','SandiA2020'),
    subject='Error OPRA-TSP'
)
mail_handler.setLevel(logging.ERROR)
mail_handler.setFormatter(logging.Formatter(
    '[%(asctime)s] %(levelname)s in %(module)s: %(message)s'
))

app = Flask(__name__)
CORS(app)
#app.config["DEBUG"] = True
app.logger.removeHandler(default_handler)
app.logger.addHandler(mail_handler)

import FlaskWebProject1.views



