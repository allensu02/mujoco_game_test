using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;
using StackExchange.Redis;
using Newtonsoft.Json.Linq;

namespace BRANDForUnity
{
    public class BRANDNode
    {
        // TODO: convert to private and create getters/setters when done developing
        public string m_redisHost;
        public int m_redisPort;
        public string m_redisSocket;
        public string m_name;

        dynamic m_nodeParams;

        ConnectionMultiplexer m_redis;
        IDatabase m_db;
        string m_supergraphId = "0-0";

        public BRANDNode(string[] args)
        {
            //#if UNITY_EDITOR
            //            // string default = "-n test -p 6380";
            //            // ParseRedisArguments(default.Split(new char[]{' ', '\n'}));
            //            SetDefaultRedisArguments();
            //#else
            //            ParseRedisArguments(System.Environment.GetCommandLineArgs());
            //#endif

            ParseRedisArguments(args);
            ConnectToRedis();
            InitializeParameters();
        }

        void SetDefaultRedisArguments()
        {
            m_name = "water_flow"; // TODO: change to some default name before committing
            m_redisHost = "localhost";
            m_redisPort = 6380;
        }

        void ParseRedisArguments(string[] args)
        {
            Log(string.Format("Length of args: {0}", args.Length));
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Contains("-n") || args[i].Contains("--nickname"))
                {
                    m_name = args[i + 1];
                }

                if (args[i].Contains("-i") || args[i].Contains("--redis_host"))
                {
                    m_redisHost = args[i + 1];
                }

                if (args[i].Contains("-p") || args[i].Contains("--redis_port"))
                {
                    m_redisPort = int.Parse(args[i + 1]);
                }

                if (args[i].Contains("-s") || args[i].Contains("--redis_socket"))
                {
                    m_redisSocket = args[i + 1];
                }
            }

            if (m_name == null || m_redisHost == null || m_redisPort == 0)
            {
                throw new ArgumentException("Missing required argument(s)");
            }
        }

        void ConnectToRedis()
        {
            Log("Connecting to Redis...");
            ConfigurationOptions config = new ConfigurationOptions
            {
                EndPoints =
                {
                    {(m_redisSocket != null) ? m_redisSocket : m_redisHost, m_redisPort }
                }
            };

            try
            {
                m_redis = ConnectionMultiplexer.Connect(config);
                m_db = m_redis.GetDatabase();
            }
            catch (RedisConnectionException ex)
            {
                throw new BRANDException("Failed to connect to Redis. Check if the Redis server was started.", ex);
            }

            Log("Connected to Redis.");

            // log status synchronously
            var res = m_db.StreamAdd(m_name + "_state",
                new NameValueEntry[]
                { new NameValueEntry("code", 0), new NameValueEntry("status", "initialized") }
            );
        }

        protected dynamic GetParametersFromSupergraph()
        {
            dynamic nodeParameters = null;

            var streamEntries = m_db.StreamRange("supergraph_stream",
                minId: m_supergraphId,
                maxId: "+",
                count: 1
            );

            if (!streamEntries.Any()) return nodeParameters;

            m_supergraphId = streamEntries[0].Id;

            foreach (var streamEntry in streamEntries)
            {
                var m_model = ParseResult(streamEntry);

                Log(string.Format("{0}", m_model.data.nodes.GetType()));

                foreach (JProperty n in m_model.data.nodes)
                {
                    if (m_name.Equals(n.Name))
                    {
                        dynamic node = n.Value;
                        nodeParameters = node.parameters;
                        return nodeParameters;
                    }
                }
            }

            return nodeParameters;
        }

        void InitializeParameters()
        {
            dynamic nodeParams = GetParametersFromSupergraph();

            if (nodeParams == null) return; // TODO: throw error instead
            m_nodeParams = nodeParams;
        }

        protected dynamic ParseResult(StreamEntry streamEntry)
        {
            dynamic parsed = new JObject();
            foreach (var ele in streamEntry.Values)
            {
                parsed.Add(ele.Name, JObject.Parse(ele.Value));
            }
            return parsed;
        }

        void Log(string message)
        {
            UnityEngine.Debug.LogFormat("[{0}] {1}", m_name, message);

        }
    }
}
