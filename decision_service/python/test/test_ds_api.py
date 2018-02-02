import sys
import unittest
import socket
from threading import Thread
import json
import ssl
from time import sleep
from http.server import BaseHTTPRequestHandler, HTTPServer
import os
import numpy.testing as npt
import numpy as np

from decision_service import *

def get_free_port():
	s = socket.socket(socket.AF_INET, type=socket.SOCK_STREAM)
	s.bind(('localhost', 0))
	address, port = s.getsockname()
	s.close()
	return port

class MockHandler(BaseHTTPRequestHandler):

	def __init__(self, mock_server, *args):
		self.mock_server = mock_server
		BaseHTTPRequestHandler.__init__(self, *args)

	def do_GET_config(self):
		# Process an HTTP GET request and return a response with an HTTP 200 status.
		self.send_response(200)
		self.send_header('Content-Type', 'application/json; charset=utf-8')
		self.end_headers()

		response_content = json.dumps({"ModelBlobUri": "http://localhost/model",
			"EventHubInteractionConnectionString": "conn1",
			"EventHubObservationConnectionString": "conn2",
			"ApplicationID": "app1"
		})
		self.wfile.write(response_content.encode('utf-8'))

		self.mock_server.get = self.mock_server.get + 1

	def do_GET_config_missing_appid(self):
		# Process an HTTP GET request and return a response with an HTTP 200 status.
		self.send_response(200)
		self.send_header('Content-Type', 'application/json; charset=utf-8')
		self.end_headers()

		response_content = json.dumps({"ModelBlobUri": "http://localhost/model",
			"EventHubInteractionConnectionString": "conn1",
			"EventHubObservationConnectionString": "conn2"
		})
		self.wfile.write(response_content.encode('utf-8'))

		self.mock_server.get = self.mock_server.get + 1


	def do_GET(self):
		if self.path == "/config.json":
			return self.do_GET_config()
		
		if self.path == "/config_missing_appid.json":
			return self.do_GET_config_missing_appid()

		self.send_response(404)
		self.end_headers()

	def do_POST(self):
		self.mock_server.post = self.mock_server.post + 1

		response_code = 404
		if (len(self.mock_server.post_status_codes) == 0):
			# default behavior
			# - respond with 201 (created) for known EventHubs
			# - respond with 404 otherwise
			if self.path.startswith("/interaction") or self.path.startswith("/observation"):
				response_code = 201
		else:
			response_code = self.mock_server.post_status_codes.pop(0)

		content_len = int(self.headers.get('content-length', 0))
		post_body = self.rfile.read(content_len).decode('utf-8')

		self.mock_server.posts.append({'path': self.path,
			'response_code': response_code,
			'body': post_body,
			'content-type': self.headers.get('content-type')})

		self.send_response(response_code)

		self.end_headers()

	# disable logging to stdout
	def log_message(self, format, *args):
		return

class MockServer:
	def __init__(self, https_enabled = True, post_status_codes = []):
		self.https_enabled = https_enabled
		self.post_status_codes = post_status_codes

	def __enter__(self):
		self.get = 0
		self.post = 0
		self.posts = []

		# Configure mock server.
		self.mock_server_port = get_free_port()

		def handler(*args):
			MockHandler(self, *args)

		self.mock_server = HTTPServer(('localhost', self.mock_server_port), handler)
		proto = 'http'
		if self.https_enabled:
			certfile = os.path.join(os.path.dirname(os.path.abspath(__file__)), '../unittest.pem')
			self.mock_server.socket = ssl.wrap_socket(self.mock_server.socket,
				server_side=True,
				certfile=certfile)
			proto = 'https'

		# Start running mock server in a separate thread.
		# Daemon threads automatically shut down when the main process exits.
		self.mock_server_thread = Thread(target=self.mock_server.serve_forever)
		self.mock_server_thread.setDaemon(True)
		self.mock_server_thread.start()

		self.base_url = '{proto}://localhost:{port}/'.format(proto=proto, port=self.mock_server_port)

		return self

	def __exit__(self, exception_type, exception_value, trackback):
		self.mock_server.server_close()

class TestDecisionServiceConfiguration(unittest.TestCase):
	def test_download(self):
		# can't easily disable cert validation w/o introducing yet another method (optional params not supported)
		with MockServer(https_enabled = False) as server:
			config = DecisionServiceConfiguration_Download(server.base_url + 'config.json')

			self.assertEqual(config.model_url, 'http://localhost/model')
			self.assertEqual(config.eventhub_interaction_connection_string, 'conn1')
			self.assertEqual(config.eventhub_observation_connection_string, 'conn2')
			self.assertEqual(config.app_id, 'app1')

			# make sure it's called once'
			self.assertEqual(server.get, 1, 'HTTP server not called')

	def test_not_found(self):
		with MockServer() as server:
			with self.assertRaises(SystemError):
				DecisionServiceConfiguration_Download(server.base_url + 'notfound')

	def test_app(self):
		with MockServer() as server:
			with self.assertRaises(SystemError):
				DecisionServiceConfiguration_Download(server.base_url + 'config_missing_appid.json')

class TestDecisionServiceLogger(DecisionServiceLogger):
	def __init__(self):
		self.messages = []
		DecisionServiceLogger.__init__(self)

	def log(self, level, msg):
		self.messages.append({'level':level, 'msg': msg})

class TestDecisionServiceClient(unittest.TestCase):
	def __init__(self, *args):
		self.key = "SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=0t45SYSay1Rm8U4riSfgaaLIIKWPMzqJILmxpAQdOHg=" 
		self.config = DecisionServiceConfiguration()

		self.config.batching_timeout_in_milliseconds = 100
		self.config.certificate_validation_enabled = False

		unittest.TestCase.__init__(self, *args)

	def test_cert_validation_fails(self):
		with MockServer() as server:
			self.config.eventhub_interaction_connection_string = "Endpoint=sb://localhost:%d/;%s;EntityPath=interaction" % (server.mock_server_port, self.key)
			self.config.eventhub_observation_connection_string = "Endpoint=sb://localhost:%d/;%s;EntityPath=observation" % (server.mock_server_port, self.key)
			# self.listener = 
			# C++ owns the object by default. Read http://www.swig.org/Doc3.0/Python.html#Python_nn35 on how to reverse
			# TODO: I don't understand how this work...
			logger = TestDecisionServiceLogger()
			self.config.logger = logger

			# make sure it's enabled and we'll internally throw
			self.config.certificate_validation_enabled = True

			client = DecisionServiceClient(self.config)
			ranking = client.explore_and_log('{"a":2}', '', [1,2,3])

			sleep(0.2)

			self.assertEqual(len(server.posts), 0, "requests should not hit the server as cert validation should fail")
			self.assertEqual(logger.messages, [{'level':1, 'msg':"Failed to upload event: 'Error in SSL handshake'"}])

	def test_rank_event_id_generated(self):
		with MockServer() as server:
			self.config.eventhub_interaction_connection_string = "Endpoint=sb://localhost:%d/;%s;EntityPath=interaction" % (server.mock_server_port, self.key)
			self.config.eventhub_observation_connection_string = "Endpoint=sb://localhost:%d/;%s;EntityPath=observation" % (server.mock_server_port, self.key)
			self.config.explorer = EpsilonGreedyExplorer(0.4)

			client = DecisionServiceClient(self.config)
			ranking = client.explore_and_log('{"a":2}', 'ab', [1,2])

			sleep(0.5)
						
			self.assertEqual(len(server.posts), 1)
			self.assertEqual(server.posts[0]['path'], '/interaction/messages?timeout=60&api-version=2014-01')
			self.assertEqual(server.posts[0]['response_code'], 201)
			self.assertEqual(server.posts[0]['content-type'], 'application/atom+xml;type=entry;charset=utf-8')
			self.assertEqual(server.posts[0]['body'], '{"Version":"1","EventId":"' + ranking.event_id + '","a":[1,0],"c":{"a":2},"p":[0.8,0.2],"VWState":{"m":"m1"}}')

	def test_rank1(self):
		with MockServer() as server:
			self.config.eventhub_interaction_connection_string = "Endpoint=sb://localhost:%d/;%s;EntityPath=interaction" % (server.mock_server_port, self.key)
			self.config.eventhub_observation_connection_string = "Endpoint=sb://localhost:%d/;%s;EntityPath=observation" % (server.mock_server_port, self.key)

			client = DecisionServiceClient(self.config)
			ranking = client.explore_and_log('{"a":2}', 'abc', [2,1,3,4])

			sleep(0.5)
						
			self.assertEqual(len(server.posts), 1)
			self.assertEqual(server.posts[0]['path'], '/interaction/messages?timeout=60&api-version=2014-01')
			self.assertEqual(server.posts[0]['response_code'], 201)
			self.assertEqual(server.posts[0]['content-type'], 'application/atom+xml;type=entry;charset=utf-8')
			self.assertEqual(server.posts[0]['body'], '{"Version":"1","EventId":"abc","a":[3,2,0,1],"c":{"a":2},"p":[0.85,0.05,0.05,0.05],"VWState":{"m":"m1"}}')

	def test_update_model(self):
		with MockServer() as server:
			self.config.eventhub_interaction_connection_string = "Endpoint=sb://localhost:%d/;%s;EntityPath=interaction" % (server.mock_server_port, self.key)
			self.config.eventhub_observation_connection_string = "Endpoint=sb://localhost:%d/;%s;EntityPath=observation" % (server.mock_server_port, self.key)
			logger = TestDecisionServiceLogger()
			self.config.logger = logger
			self.config.log_level = 4
			self.config.explorer = EpsilonGreedyExplorer(0.3)

			client = DecisionServiceClient(self.config)

			ranking = client.explore_and_log('{"a":2}', '', [1,2])

			# buf = bytearray(b'foo')
			# client.update_model(buf)

			# self.assertEqual(logger.messages, [{'level':1, 'msg':'update_model(len=3)'}])

	def test_rank2(self):
		class TestPredictor(DecisionServicePredictors):
			def __init__(self):
				# self.messages = []
				DecisionServicePredictors.__init__(self)

			def get_prediction(self, index, previous_decisions):
				return [.1,.2]

		with MockServer() as server:
			self.config.eventhub_interaction_connection_string = "Endpoint=sb://localhost:%d/;%s;EntityPath=interaction" % (server.mock_server_port, self.key)
			self.config.eventhub_observation_connection_string = "Endpoint=sb://localhost:%d/;%s;EntityPath=observation" % (server.mock_server_port, self.key)
			logger = TestDecisionServiceLogger()
			self.config.logger = logger
			self.config.log_level = 4

			preds = TestPredictor()
			client = DecisionServiceClient(self.config)
			# TODO: debug me, segfaults!
			# ranking = client.explore_and_log('{"a":2}', '', preds)

class TestDecisionServiceClient(unittest.TestCase):
	def test_epsilon_greedy(self):
		distribution = ExplorationStrategies_epsilon_greedy(0.4, 1, 4)

		npt.assert_allclose(distribution, [0.1,0.7,0.1,0.1])

	def test_softmax(self):
		distribution = ExplorationStrategies_softmax(1.2, [.2,.3,.4])

		npt.assert_allclose(distribution, [0.294226,  0.331739,  0.374035], rtol=1e-04)

	def test_bag(self):
		distribution = ExplorationStrategies_bag([0,2,0,3], 4)

		npt.assert_allclose(distribution, [0.5, 0, 0.25, 0.25])

	def test_enforce_minimum_probability(self):
		distribution = ExplorationStrategies_enforce_minimum_probability(0.3, [0.1, 0.9])

		# each action is explored with at least 0.3/2 prob
		npt.assert_allclose(distribution, [0.15, 0.85])

	def test_choose_action(self):
		sampling = Sampling()

		action = sampling.choose_action("123", [1,0])
		self.assertEqual(action, 0)

		action = sampling.choose_action("123", [0,1])
		self.assertEqual(action, 1)

		# TODO: test that this approaches certain distribution?
		# generate GUID

	def test_rank(self):
		sampling = Sampling("app1")

		rank = sampling.rank("123", [1, 0], [0, 1])
		npt.assert_array_equal(rank, [0, 1])

	def test_rank2(self):
		sampling = Sampling("app1")

		rank = sampling.rank("123", [.2, .2, .6], [2, 0, 1])
		npt.assert_array_equal(rank, [1, 0, 2])

		rank = sampling.rank_by_score("123", [.2, .2, .6], [0.4,.1,.8])
		npt.assert_array_equal(rank, [2, 0, 1])

	def test_flow1(self):
		# model prediction
		# 2,1,0
		scores = [.2, .4, .8]

		top_action = scores.index(max(scores))
		sampling = Sampling("app1")

		epsilon = 0.5
		distribution = ExplorationStrategies_epsilon_greedy(epsilon, top_action, len(scores))

		# print(distribution)
		# keep around to cache hash(app1)

		rank_distr = np.zeros((3,3))
		n = 10000
		for i in range(0,n):
			event_id = "prefix%d" % i

			# sort action ids by scores
			rank = sampling.rank_by_score(event_id, distribution, scores)
			for p in range(0,3):
				# print("slot: %d action: %d" % (p, rank[p]))
				rank_distr[p, rank[p]] = rank_distr[p, rank[p]] +1

		rank_distr = rank_distr/n
		# print(rank_distr)
		# "top slot distribution must converge to specified distribution"
		npt.assert_allclose(rank_distr[0,], distribution, atol=0.02)
		
		self.assertEqual(rank_distr[1,2], 0, "slot 1 can never get action 2")
		self.assertEqual(rank_distr[2,1], 0, "slot 2 can never get action 1")

		# npt.assert_array_equal(rank, [2, 0, 1])

if __name__ == '__main__':
	unittest.main()
