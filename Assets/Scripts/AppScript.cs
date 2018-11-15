using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Concurrent;

public class AppScript : MonoBehaviour
{

    public Transform controllerModel;
    OSCServer _oscServer;

    long _latencySum = 0;
    long _framesCount = 0;


    ConcurrentStack<Quaternion> _quaternionStack;
    Quaternion _lastAvailableQuaternion;
    Quaternion _zeroQuaternion = Quaternion.identity;

    long _lastTimestampReceived = 0;
    
    long _firstPacketReceivedTimestamp = -1;
    // Start is called before the first frame update
    void Start()
    {
        _quaternionStack = new ConcurrentStack<Quaternion>();
        
        Application.targetFrameRate = 60;

        _oscServer = new OSCServer(7003);
        _oscServer.PacketReceivedEvent += (s, packet) => {

            
            
            if(packet.Address == "/c1"){
                // json approach
                var str = packet.Data[0] as string;
                var timestamp = (long)packet.Data[1];

            //     if(timestamp < _lastTimestampReceived){
            //         Debug.LogWarning("Receieved earlier timestamp");
            //         return;
            //     }

            //     if(_firstPacketReceivedTimestamp == -1){
            //         _firstPacketReceivedTimestamp = timestamp;
            //     }
            //     _lastTimestampReceived = timestamp;

            //     var latencyMs = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            //                     - timestamp;
            //     _latencySum += latencyMs;
            //     _framesCount++;

            //     var deltaSeconds = (System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _firstPacketReceivedTimestamp)/1000f;

            //     Debug.Log($"Network FPS: {(float)_framesCount/deltaSeconds}");

            //    Debug.Log($"OSC network latencyMs: {latencyMs}, avg: {(float)_latencySum/_framesCount}");

                // var stopwatch = new System.Diagnostics.Stopwatch();
                // stopwatch.Start();
                var trackingData = JsonConvert.DeserializeObject<GearVRControllerTrackingData>(str);
                // Debug.Log($"Time to parse json data: {stopwatch.Elapsed.TotalMilliseconds} ms"); // ~0.1ms

                

                _lastAvailableQuaternion = new Quaternion(
                    trackingData.quaternion[0],
                    trackingData.quaternion[1],
                    trackingData.quaternion[2],
                    trackingData.quaternion[3]
                );


                //_lastAvailableQuaternion = Quaternion.Inverse(_lastAvailableQuaternion);
                

                // Loom.QueueOnMainThread(() => {
                    
                // });
            }           
            
        };

        _oscServer.Connect();
    }


    

    // Update is called once per frame
    void Update()
    {        
       
       if(Input.GetKeyDown(KeyCode.Space)){
           Debug.Log("Reset");
           _zeroQuaternion = Quaternion.Inverse(_lastAvailableQuaternion);
       }

       controllerModel.localRotation = _zeroQuaternion * _lastAvailableQuaternion;
            
    }

    // EVENTS

    void OnDestroy(){
        if(_oscServer != null){
            _oscServer.Close();            
            _oscServer = null;
        }
    }
}
