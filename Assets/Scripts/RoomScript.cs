using UnityEngine;

public class RoomScript : MonoBehaviour
{
	private int roomNumber = -1;

	void Start() 
	{
		roomNumber = int.Parse(name.Substring(4));
	}
	
	public void RoomClicked()
	{
		GameObject.Find("Map").GetComponent<MapScript>().Move(roomNumber);			
	}
}
