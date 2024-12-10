using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using StackExchange.Redis;
using System.Linq;
using System.Threading.Tasks;
// using BRAND;
using BRAND.Unity;
using StackExchange.Redis;
using Newtonsoft.Json.Linq;

public class Ball : MonoBehaviour
{
    private string lastId;
    private string streamKey;
    private BRANDNode node;

    // ConnectionMultiplexer redis;
    // IDatabase db;

    // Dictionary<string, string> ParseResult(StreamEntry entry) => entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());

    void Awake()
    {
        // string args = "-n waterflow -i localhost -p 6380";
        // BRANDNode test = new BRANDNode(args.Split(' '));

        node = new BRANDNode("water_flow", "localhost", 6380, null);
        lastId = "-";
    }
    void Start()
    {
        // redis = ConnectionMultiplexer.Connect("localhost");
        // db = redis.GetDatabase();
    }

    async void Update()
    {
        // foreach (var e in serialized)
        // {
        //     var abc = new JValue(e.Value);
        //     node.Log(string.Format("testing UnboxPrimitives: {0}", abc.Type));
        // }

        node.Log("after StreamAdd");

        dynamic value = await GetRedisValue("command");

        node.Log("after GetRedisValue");

        node.Log(string.Format("type test: {0}", value.someKey.Value is string));

        node.Log(string.Format("node params type test: {0}", node.Params.width is JValue));

        // JValue test = new JValue(123);
        
        // Test(test);

    }

    // RedisValue Test(JValue obj)
    // {
    //     switch (obj)
    //     {
    //         case string v: return v;
    //         case int v: return v;
    //         case uint v: return v;
    //         case double v: return v;
    //         case byte[] v: return v;
    //         case bool v: return v;
    //         case long v: return v;
    //         case ulong v: return v;
    //         case float v: return v;
    //         case RedisValue v: return v;
    //         default:
    //             valid = false;
    //             return Null;
    //     }
    // }

    // Update is called once per frame
    // async void Update()
    // {
    //     var streamRangeTask = Task.Run(async() => {
    //         var key = "supervisor_ipstream";
    //         var result = await db.StreamRangeAsync(key, "-", "+", 1, Order.Descending);
    //         if (result.Any())
    //         {
    //             foreach (var entry in result)
    //             {
    //                 var dict = ParseResult(entry);
    //                 UnityEngine.Debug.LogFormat("id: {0}", entry.Id);
    //                 foreach (var ele in dict)
    //                 {
    //                     UnityEngine.Debug.LogFormat("key: {0}, value: {1}", ele.Key, ele.Value);
    //                 }
    //             }
    //         }
    //     });
        
    //     await Task.WhenAll(streamRangeTask).ObserveErrors();
    // }

    public async Task<dynamic> GetRedisValue(string key)
    {
        var result = await node.Db.StreamRangeAsync(key, lastId, "+", 1, Order.Descending);

        if (result.Any())
        {
            StreamEntry streamEntry = result[0];
            lastId = streamEntry.Id;

            RedisValue rv = streamEntry.Values[0].Value;

            // int? i = (int?) rv;

            node.Log(string.Format("{0}", rv));
            var parsed = BRANDNode.DeserializeStreamEntry(streamEntry);

            return parsed;


            // if (value.Equals("LIGHT"))
            // {
                
            //     wasLight = true;
            //     node.Log("LIGHT");
            //     cameraRef.backgroundColor = whiteColor;
            //     // node.Log()
            //     // cameraRef.backgroundColor = Color.white;
            // } 
            // else if (value.Equals("DARK")) {
            //     wasLight = false;
            //     node.Log("DARK");
            // }
        }

        dynamic empty = new JObject();
        empty.someKey = "nothing";

        return empty;

    }

    // public async Task GetRedisValue()
    // {
    //     var key = "supervisor_ipstream";
    //     var result = await db.StreamRangeAsync(key, "-", "+", 1, Order.Descending);
    //     if (result.Any())
    //     {
    //         var dict = ParseResult(result.First());
    //         foreach (var ele in dict)
    //         {
    //             UnityEngine.Debug.Log(ele.Value);
    //         }
    //     }
    // }
}
