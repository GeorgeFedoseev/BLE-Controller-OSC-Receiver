using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Concurrent;

public class AppScript : MonoBehaviour
{


    public Transform controllersContainer;


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

            Debug.Log($"received OSC {packet.Address}");

            Loom.QueueOnMainThread(() => {
                var controllerTransform = controllersContainer.Find(packet.Address.Replace("/", ""));

                if(controllerTransform != null){
                    // json approach
                    var str = packet.Data[0] as string;
                    var timestamp = (long)packet.Data[1];

                    var trackingData = JsonConvert.DeserializeObject<GearVRControllerTrackingData>(str);
                    

                    _lastAvailableQuaternion = new Quaternion(
                        trackingData.quaternion[0],
                        trackingData.quaternion[1],
                        trackingData.quaternion[2],
                        trackingData.quaternion[3]
                    );                    
                    
                    controllerTransform.localRotation = _zeroQuaternion * _lastAvailableQuaternion;                
                }
            });
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
            
    }

    // EVENTS

    void OnDestroy(){
        if(_oscServer != null){
            _oscServer.Close();            
            _oscServer = null;
        }
    }
}
