using UnityEditor;
 using UnityEngine;
 using System.Collections.Generic;

 namespace DoodleStudio95 {
 // makes sure that the static constructor is always called in the editor.
//  [InitializeOnLoad]
 public static class AnimationFileDragHandler
 {
     static AnimationFileDragHandler ()
     {
			 Debug.Log("hi");
         // Adds a callback for when the hierarchy window processes GUI events
         // for every GameObject in the heirarchy.
         EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
         EditorApplication.hierarchyChanged += hierarchyWindowChanged;
     }
 
     static void hierarchyWindowChanged() {
			 Debug.Log("hierarchyWindowChanged");
		 }
     static void HierarchyWindowItemOnGUI(int pID, Rect pRect)
     {
			 
         // happens when an acceptable item is released over the GUI window
        //  if (Event.current.type == EventType.DragPerform)
        //  if (Event.current.type == EventType.DragExited)
        //  {
				// 	 Debug.Log("drag perform "  + pRect.Contains(Event.current.mousePosition));
				//  }
				//  if (false) {
        //      // get all the drag and drop information ready for processing.
        //      DragAndDrop.AcceptDrag();
        //      // used to emulate selection of new objects.
        //      var selectedObjects = new List<GameObject>();
        //      // run through each object that was dragged in.
        //      foreach (var objectRef in DragAndDrop.objectReferences)
        //      {
				// 			 Debug.Log(objectRef);
        //          // if the object is the particular asset type...
        //          if (objectRef is DoodleAnimationFile)
        //          {
        //              var obj = (objectRef as DoodleAnimationFile).Make3DSprite();
        //              // add to the list of selected objects.
        //              selectedObjects.Add(obj);
        //          }
        //      }
        //      // we didn't drag any assets of type AssetX, so do nothing.
        //      if (selectedObjects.Count == 0) return;
        //      // emulate selection of newly created objects.
        //      Selection.objects = selectedObjects.ToArray();
 
        //      // make sure this call is the only one that processes the event.
        //      Event.current.Use();
        //  }
     }
 }
 }