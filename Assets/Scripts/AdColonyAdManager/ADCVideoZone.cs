using UnityEngine;
using System.Collections;

#if !UNITY_IPHONE

public class ADCVideoZone {

  public string zoneId = "";
  public ADCVideoZoneType zoneType = ADCVideoZoneType.None;

  public ADCVideoZone(string newZoneId, ADCVideoZoneType newVideoZoneType) {
    zoneId = newZoneId;
    zoneType = newVideoZoneType;
	}
}

#endif