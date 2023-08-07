using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MapScript : MonoBehaviour
{
	public Sprite[] patternArray;
	private static Color[] colorArray = new Color[] { Color.grey, Color.red, Color.yellow, Color.blue };
	public static readonly int mapWidth = 5;
	public static readonly int mapHeight = 5;
	private List<(Sprite, Color)> currentKeyList = new ();
	private List<(Sprite, Color)> initialKeyList = new ();
	public enum DIFFICULTY { EASY, MEDIUM, HARD };
	private DIFFICULTY currentDifficulty = DIFFICULTY.EASY;
	public GameObject keyPrefab;
	private readonly int keysInColumn = 8;
	private static readonly float keyStartX = -1.25f;
	private static readonly float keySpacingX = 1.25f;
	private static readonly float keyStartY = 3f;
	private static readonly float keySpacingY = -1f;
	public int playerLocation = 0;
	private Dictionary<string, (Sprite, Color)> initialCorridors = new();

	void Start()
	{
		var difficultyMode = PlayerPrefs.GetString("mode");
		switch (difficultyMode)
		{
			case "easy":
				GenerateMap(DIFFICULTY.EASY);
				break;
			case "medium":
				GenerateMap(DIFFICULTY.MEDIUM);
				break;
			case "hard":
				GenerateMap(DIFFICULTY.HARD);
				break;
		}
	}

	void Update() 
	{
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
			if(hit.collider != null && hit.collider.gameObject.tag == "Room")
			{
				GameObject.Find(hit.collider.gameObject.name).GetComponent<RoomScript>().RoomClicked();		
			}
		}	
	}

	public void Move(int roomNumber)
	{
		if (currentKeyList.Count == 0)
		{
			return;
		}
		if (Mathf.Abs(roomNumber - playerLocation) != 1 && Mathf.Abs(roomNumber - playerLocation) != 5)
		{
			return;	
		}
		if (Mathf.Abs(roomNumber - playerLocation) == 1 
			&& roomNumber / MapScript.mapWidth != playerLocation / MapScript.mapWidth)
		{
			return;
		}
		var room1 = Mathf.Min(roomNumber, playerLocation);
		var room2 = Mathf.Max(roomNumber, playerLocation);
		var corridorName = "Corridor" + room1.ToString("D2") + "-" + room2.ToString("D2");
		if (GameObject.Find(corridorName).GetComponent<SpriteRenderer>().color 
			!= GameObject.Find("Map").GetComponent<MapScript>().GetNextColor())
		{
			return;
		}
		if (currentDifficulty == DIFFICULTY.EASY || currentDifficulty == DIFFICULTY.MEDIUM)
		{
			Prune(playerLocation, true);
		}
		var x = (roomNumber % mapWidth) * 2f + .5f;
		var y = .5f - (roomNumber / mapWidth) * 2f;
		GameObject.Find("Player").transform.localPosition = new Vector3(x, y, -5);
		playerLocation = roomNumber;
		if (currentDifficulty == DIFFICULTY.EASY)
		{
			Prune(playerLocation, false);
		}
		RemoveFirstKey();
		if (playerLocation == mapWidth * mapHeight - 1 && currentKeyList.Count == 0)
		{
			GenerateMap(currentDifficulty);
		}
	}

	private void Prune(int roomNumber, bool includeAll)
	{
		if (roomNumber % mapWidth > 0)
		{
			var corridorName = "Corridor" 
				+ (roomNumber - 1).ToString("D2") + "-" + roomNumber.ToString("D2");
			RemoveCorridor(corridorName);
		}
		if (roomNumber % mapWidth < mapWidth - 1 && includeAll)
		{
			var corridorName = "Corridor" 
				+ roomNumber.ToString("D2") + "-" + (roomNumber + 1).ToString("D2");
			RemoveCorridor(corridorName);
		}
		if (roomNumber / mapWidth > 0)
		{
			var corridorName = "Corridor" 
				+ (roomNumber - mapWidth).ToString("D2") + "-" + roomNumber.ToString("D2");
			RemoveCorridor(corridorName);
		}
		if (roomNumber / mapWidth < mapHeight - 1 && includeAll)
		{
			var corridorName = "Corridor" 
				+ roomNumber.ToString("D2") + "-" + (roomNumber + mapWidth).ToString("D2");
			RemoveCorridor(corridorName);
		}
	}

	public void ResetMap() 
	{
		playerLocation = 0;
		GameObject.Find("Player").transform.localPosition = new Vector3(.5f, .5f, -5);
		foreach (string thisCorridorName in initialCorridors.Keys)
		{
			var thisSpriteRenderer = GameObject.Find(thisCorridorName).GetComponent<SpriteRenderer>();
			thisSpriteRenderer.sprite = initialCorridors[thisCorridorName].Item1;
			thisSpriteRenderer.color = initialCorridors[thisCorridorName].Item2;
		}
		currentKeyList = new (initialKeyList);
		UpdateKeyQueue();
	}

	private void RandomizeColors()
	{
		var corridors = GameObject.FindGameObjectsWithTag("Corridor");
		foreach (GameObject thisCorridor in corridors)
		{
			var choice = (int)Mathf.Round((patternArray.Length - 2) * Random.value) + 1;
			thisCorridor.GetComponent<SpriteRenderer>().sprite = patternArray[choice];
			thisCorridor.GetComponent<SpriteRenderer>().color = colorArray[choice];
			initialCorridors[thisCorridor.name] = (patternArray[choice], colorArray[choice]);
		}
	}

	public void RemoveCorridor(string corridorName)
	{
		var spriteRenderer = GameObject.Find(corridorName).GetComponent<SpriteRenderer>();
		spriteRenderer.sprite = patternArray[0];
		spriteRenderer.color = colorArray[0];
	}

	public Color GetNextColor()
	{
		return currentKeyList[0].Item2;
	}

	public void RemoveFirstKey()
	{
		currentKeyList.RemoveAt(0);
		UpdateKeyQueue();
	}

	public void GenerateMap(DIFFICULTY difficulty)
	{
		currentDifficulty = difficulty;
		RandomizeColors();
		var targetRoomCount = mapWidth + mapHeight - 1;
		if (difficulty == DIFFICULTY.MEDIUM)
		{
			targetRoomCount += 4;
		}
		else if (difficulty == DIFFICULTY.HARD)
		{
			targetRoomCount = mapWidth * mapHeight - 4;
		}
		initialKeyList.Clear();
		var path = GenerateMap(new List<int>() {0 }, new HashSet<int>(), difficulty, targetRoomCount);
		for (int i = 1; i < path.Count; i++)
		{
			var room1 = Mathf.Min(path[i - 1], path[i]);
			var room2 = Mathf.Max(path[i - 1], path[i]);
			var corridorName = "Corridor" + room1.ToString("D2") + "-" + room2.ToString("D2");
			var thisSprite = GameObject.Find(corridorName).GetComponent<SpriteRenderer>().sprite;
			var thisColor = GameObject.Find(corridorName).GetComponent<SpriteRenderer>().color;
			initialKeyList.Add((thisSprite, thisColor));
		}
		ResetMap();
		UpdateKeyQueue();
	}

	private List<int> GenerateMap(
		List<int> roomList, HashSet<int> visited, DIFFICULTY difficulty, int targetRoomCount)
	{
		var nextVisited = new HashSet<int>(visited);
		var currentRoom = roomList.Last();
		if (currentRoom == mapWidth * mapHeight - 1)
		{
			return roomList;
		}
		var nextRoomOptions = new List<int>();
		nextVisited.Add(currentRoom);
		if (currentRoom % mapWidth > 0 && !visited.Contains(currentRoom - 1) && difficulty != DIFFICULTY.EASY)
		{
			nextRoomOptions.Add(currentRoom - 1);
		}
		if (currentRoom % mapWidth < mapWidth - 1 && !visited.Contains(currentRoom + 1))
		{
			nextRoomOptions.Add(currentRoom + 1);
		}		
		if (currentRoom / mapHeight > 0 && !visited.Contains(currentRoom - mapWidth) && difficulty != DIFFICULTY.EASY)
		{
			nextRoomOptions.Add(currentRoom - mapWidth);
		}
		if (currentRoom / mapHeight < mapHeight - 1 && !visited.Contains(currentRoom + mapWidth))
		{
			nextRoomOptions.Add(currentRoom + mapWidth);
		}
		if (nextRoomOptions.Count == 0)
		{
			return new List<int>();
		}
		nextRoomOptions = nextRoomOptions.OrderBy(x => Random.value).ToList();
		foreach (int thisNextRoom in nextRoomOptions)
		{
			var nextRoomList = new List<int>(roomList);
			nextRoomList.Add(thisNextRoom);
			var thisNextRoomList = GenerateMap(nextRoomList, nextVisited, difficulty, targetRoomCount);
			if (thisNextRoomList.Count == targetRoomCount)
			{
				return thisNextRoomList;
			}
		}
		return new List<int>();
	}

	public void UpdateKeyQueue()
	{
		var allKeys = GameObject.FindGameObjectsWithTag("Key");
		foreach (GameObject thisKey in allKeys)
		{
			Destroy(thisKey);
		}	
		var keyQueueContent = GameObject.Find("KeyQueueContent");
		for (int i = 0; i < currentKeyList.Count; i++)
		{
			var newKey = Instantiate(keyPrefab);
			newKey.transform.SetParent(keyQueueContent.transform);
			newKey.transform.localScale = new Vector3(.5f, .5f, 1f);
			var xPosition = i == 0 
				? keyStartX 
				: (i - 1) / keysInColumn * keySpacingX + keyStartX;
			var yPosition = i == 0
				? keyStartY - keySpacingY
				: ((i - 1) % keysInColumn) * keySpacingY + keyStartY;
			newKey.transform.localPosition = new Vector3(xPosition, yPosition, -1);
			newKey.GetComponent<SpriteRenderer>().sprite = currentKeyList[i].Item1;
			newKey.GetComponent<SpriteRenderer>().color = currentKeyList[i].Item2;
		}		
	}

}
