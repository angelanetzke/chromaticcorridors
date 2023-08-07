using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour
{
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
			if(hit.collider != null && hit.collider.gameObject.tag == "Button")
			{
				GameObject.Find(hit.collider.gameObject.name).GetComponent<ButtonScript>().ButtonClicked();	
			}
		}
	}

	public void ButtonClicked()
	{
		if (SceneManager.GetActiveScene().name == "HomeScreenScene" 
			|| SceneManager.GetActiveScene().name == "HelpScreenScene")
		{
			GameObject.Find("Loading").GetComponent<SpriteRenderer>().enabled = true;
		}
		if (name == "ButtonReset")
		{
			GameObject.Find("Map").GetComponent<MapScript>().ResetMap();
		}
		else if (name == "ButtonEasy")
		{
			GameObject.Find("Map").GetComponent<MapScript>().GenerateMap(MapScript.DIFFICULTY.EASY);
		}
		else if (name == "ButtonMedium")
		{
			GameObject.Find("Map").GetComponent<MapScript>().GenerateMap(MapScript.DIFFICULTY.MEDIUM);
		}
		else if (name == "ButtonHard")
		{
			GameObject.Find("Map").GetComponent<MapScript>().GenerateMap(MapScript.DIFFICULTY.HARD);
		}
		else if (name == "ButtonEasyOther")
		{
			PlayerPrefs.SetString("mode", "easy");
			SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
		}
		else if (name == "ButtonMediumOther")
		{
			PlayerPrefs.SetString("mode", "medium");
			SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
		}
		else if (name == "ButtonHardOther")
		{
			PlayerPrefs.SetString("mode", "hard");
			SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
		}
		else if (name == "ButtonBack")
		{
			SceneManager.LoadScene("HomeScreenScene", LoadSceneMode.Single);
		}
		else if (name == "ButtonHowToPlay")
		{
			SceneManager.LoadScene("HelpScreenScene", LoadSceneMode.Single);
		}
	}
}
