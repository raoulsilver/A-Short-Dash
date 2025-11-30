using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DoodleStudio95 {
// A rect that handles mouse input when pressing inside and dragging out
// It moves to "Dragging" state when mouse press starts inside and stops
// when the mouse leaves
internal class InputRect {

	
	internal enum MouseState {
		Uninitialized,
		Idle,
		Hovering,
		StartedPressing,
		Pressing,
		Dragging,
		StoppedPressing,
	}

	static Dictionary<MouseState, Color> DEBUG_COLORS = new Dictionary<MouseState, Color>() {
		{ MouseState.Uninitialized, Color.red },
		{ MouseState.Idle, Color.gray },
		{ MouseState.Hovering, Color.yellow },
		{ MouseState.StartedPressing, Color.cyan },
		{ MouseState.Pressing, new Color(0,0.8f,1) },
		{ MouseState.Dragging, Color.green },
		{ MouseState.StoppedPressing, Color.magenta },
	};

	static EventType[] STATE_EVENTS = new EventType[] {
		EventType.MouseDown, EventType.MouseUp, EventType.MouseDrag,
		EventType.MouseEnterWindow, EventType.MouseLeaveWindow,
		EventType.DragPerform, EventType.DragExited, EventType.MouseDrag,
		EventType.Repaint
	};

	internal System.Action OnStartedPressing = null;
	internal System.Action OnStoppedPressing = null;
	internal System.Action OnDragged = null;

	private MouseState m_State = MouseState.Uninitialized;
	private Vector2 m_LastMousePosition;

	internal Vector2 LastMousePosition { get { return m_LastMousePosition; } }
	internal MouseState State { get { return m_State; } }
	internal bool IsDragging { get { return m_State == MouseState.StartedPressing || m_State == MouseState.Dragging; } }
	internal bool MousePressing { get { return m_State == MouseState.StartedPressing || m_State == MouseState.Pressing || m_State == MouseState.Dragging; } }
	internal bool MouseOver { get { return m_State == MouseState.Hovering || MousePressing || m_State == MouseState.StoppedPressing; } }

	internal void Update(bool inside) {
		UpdateState(inside);
	}
	internal void Update(Rect rect, string name = "") {
		bool inside = rect.Contains(Event.current.mousePosition);
		UpdateState(inside);

		DebugDraw(rect, name);
	}

	internal void DebugDraw(Rect rect, string name = "") {
		if (DrawPrefs.SHOW_DEBUG_INPUTRECTS && Event.current.type == EventType.Repaint) {
			GUI.color = DEBUG_COLORS[m_State];
			GUI.Box(rect, name + " " + m_State.ToString());
			GUI.color = Color.white;
		}
	}

	void UpdateState(bool inside) {
		var t = Event.current.type;

		// Ignore all events that don't concern us
		if (System.Array.IndexOf(STATE_EVENTS, t) == -1)
			return;
		
		switch(m_State) {
			case MouseState.Uninitialized:
			case MouseState.Idle:
				if (inside)
					m_State = MouseState.Hovering;
			break;
			case MouseState.Hovering:
				if (inside) {
					bool mouseDown = t == EventType.MouseDown;
					if (mouseDown) {
						m_State = MouseState.StartedPressing;
					}
				} else {
					m_State = MouseState.Idle;
				}
			break;
			case MouseState.StartedPressing:
				// OnStartedDragging()
				if (OnStartedPressing != null)
					OnStartedPressing.Invoke();
				m_State = MouseState.Pressing;
			break;
			case MouseState.Pressing:
			case MouseState.Dragging:
				bool mouseUp = t == EventType.MouseUp;
				bool left = t == EventType.DragExited;// || t == EventType.MouseLeaveWindow; // MouseLeaveWindow getting triggered in the middle of the image
				if(!inside || mouseUp || left) {
					m_State = MouseState.StoppedPressing;
				} else {
					if (m_LastMousePosition.Equals(Event.current.mousePosition)) {
						m_State = MouseState.Pressing;
					} else {
						m_State = MouseState.Dragging;
						if (OnDragged != null)
							OnDragged.Invoke();	
					}
				}
			break;
			case MouseState.StoppedPressing:
				// OnStoppedDragging
				if (OnStoppedPressing != null)
						OnStoppedPressing.Invoke();
				m_State = MouseState.Idle;
			break;
		}

		m_LastMousePosition = Event.current.mousePosition;
		
	}
}

// A testing window
internal class InputWindow : EditorWindow {
	// [MenuItem("Tools/Doodle Studio 95/*Testing* input window")]
	// internal static void Open() {
	// 	EditorWindow.FocusWindowIfItsOpen(typeof(InputWindow));
  //   EditorWindow.GetWindow(typeof(InputWindow), true);
	// }
	InputRect m_RectTest;
	void OnEnable() {
		m_RectTest = new InputRect();
	} 
	void OnGUI() {
		using( var s = new EditorGUILayout.VerticalScope(GUILayout.Width(50)) )
		{
			var r = GUILayoutUtility.GetRect(100,100);
			m_RectTest.Update(r);
		}
		Repaint();
	}
}

}