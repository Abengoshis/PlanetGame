using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public static class Helper
{
	public static bool IsUIBeingTouched()
	{
		UnityEngine.EventSystems.EventSystem eventSystem = UnityEngine.EventSystems.EventSystem.current;
		
		if (eventSystem.currentSelectedGameObject == null ||
		    eventSystem.currentSelectedGameObject.GetComponent<Button>() == null)
			return false;
		
		return true;
	}
}
