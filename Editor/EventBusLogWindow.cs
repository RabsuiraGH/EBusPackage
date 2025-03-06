using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace EBus.Editor
{
    public sealed class EventBusLogWindow : EditorWindow, IHasCustomMenu
    {
        private static Texture2D _darkTexture;
        private static Texture2D _lightTexture;
        private static Texture2D _selectedTexture;

        private static List<LogEntry> _eventBusLogs = new();

        private List<string> _eventBusNames = new();
        private string[] _logTypeNames = Enum.GetNames(typeof(BusLogType)).ToArray();

        private List<IEventBusLogable> _selectedBuses = new();
        private List<IEventBusLogable> _eventBuses = new();

        private bool _showTimestamp = true;
        private bool _showBusName = true;

        private int _logLimit = 100;
        private bool _clearOnPlay = false;

        private int _eventBusMask = 0;
        private int _logTypeMask = 0;

        private Vector2 _scrollPosition;
        private int _selectedIndex = -1;


        public EventBusLogWindow()
        {
            EventBusAggregator.OnLog += AddLog;
            EventBusAggregator.OnNewEventBus += HandleEventBusChange;
        }


        private void OnEnable()
        {
            _darkTexture = MakeColorTexture(new Color(0.19f, 0.19f, 0.19f));
            _lightTexture = MakeColorTexture(new Color(0.25f, 0.25f, 0.25f));
            _selectedTexture = MakeColorTexture(new Color(0.18f, 0.48f, 0.57f));

            _logTypeMask = 1;

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }


        private void OnGUI()
        {
            GUILayout.BeginHorizontal();

            // Clear button
            if (GUILayout.Button("Clear", GUILayout.Width(64)))
            {
                ClearLogs();
            }

            // Clear on play toggle
            _clearOnPlay = GUILayout.Toggle(_clearOnPlay, "Clear On Play", GUILayout.Width(96));

            GUILayout.FlexibleSpace();

            // Log filter
            EditorGUILayout.LabelField("Log Filter", GUILayout.Width(64));
            _logTypeMask = EditorGUILayout.MaskField(_logTypeMask, _logTypeNames, GUILayout.Width(128));

            // Last logs
            EditorGUILayout.LabelField("Last Logs", GUILayout.Width(64));
            _logLimit = EditorGUILayout.IntField(_logLimit, GUILayout.Width(32));

            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();

            // Listen bus
            _eventBusNames.Clear();
            _eventBusNames.AddRange(_eventBuses.Select(bus => "Event Bus " + (_eventBuses.IndexOf(bus) + 1)));

            if (!_eventBuses.Any())
            {
                _eventBusMask = 0;
                EditorGUILayout.LabelField("Listen Bus", GUILayout.Width(64));
                EditorGUILayout.LabelField("Not Available", GUILayout.Width(256));
            }
            else
            {
                EditorGUILayout.LabelField("Listen Bus", GUILayout.Width(64));
                _eventBusMask =
                    EditorGUILayout.MaskField(_eventBusMask, _eventBusNames.ToArray(), GUILayout.Width(128));
            }

            GUILayout.FlexibleSpace();

            // Get all signals
            if (GUILayout.Button("Get All Signals", GUILayout.Width(128)))
            {
                foreach (var bus in _selectedBuses)
                {
                    bus.GetAllSignals();
                }
            }

            GUILayout.EndHorizontal();

            _selectedBuses.Clear();
            for (int i = 0; i < _eventBusNames.Count; i++)
            {
                if ((_eventBusMask & (1 << i)) != 0)
                {
                    _selectedBuses.Add(_eventBuses[i]);
                }
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            // Messages
            GUIStyle style = new(GUI.skin.label)
                { richText = true, normal = { textColor = Color.white }, wordWrap = true };

            int startRange = Math.Clamp(_eventBusLogs.Count - _logLimit, 0, _eventBusLogs.Count);
            for (int i = startRange; i < _eventBusLogs.Count; i++)
            {
                if (_eventBusMask == 0) break;

                LogEntry log = _eventBusLogs[i];

                if (_selectedBuses.Count > 0 && !_selectedBuses.Contains(log.Bus) && log.Bus != null) continue;
                if ((log.Type & (BusLogType)_logTypeMask) == 0) continue;


                string timestamp = _showTimestamp ? $"[{log.Timestamp:HH:mm:ss}]" : "";
                string busPrefix = _showBusName && log.Bus != null ? $"[Bus {_eventBuses.IndexOf(log.Bus) + 1}] " : "";
                string logMessage = $"{timestamp} {busPrefix}{log.Message}";

                style.normal.textColor = log.Type switch
                {
                    BusLogType.Error => Color.red,
                    BusLogType.Warning => Color.yellow,
                    _ => Color.white
                };

                Rect labelRect = GUILayoutUtility.GetRect(new GUIContent(logMessage), style);
                if (labelRect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.MouseDown)
                    {
                        _selectedIndex = i;
                        Repaint();
                    }
                }

                if (_selectedIndex == i)
                {
                    style.normal.background = _selectedTexture;
                }
                else
                {
                    style.normal.background = (i % 2 == 0) ? _darkTexture : _lightTexture;
                }

                GUI.Label(labelRect, logMessage, style);
            }

            EditorGUILayout.EndScrollView();
        }


        [MenuItem("Window/Event Bus Logs")]
        public static void ShowWindow()
        {
            EventBusLogWindow window = GetWindow<EventBusLogWindow>("Event Bus Logs");
            window.Show();
        }


        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Show Timestamp"), _showTimestamp, ShowTimestamp);
            menu.AddItem(new GUIContent("Show Bus Name"), _showBusName, ShowBusName);
        }


        private void ShowTimestamp()
        {
            _showTimestamp = !_showTimestamp;
            Repaint();
        }


        private void ShowBusName()
        {
            _showBusName = !_showBusName;
            Repaint();
        }


        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (_clearOnPlay && state == PlayModeStateChange.EnteredPlayMode ||
                state == PlayModeStateChange.ExitingEditMode)
            {
                ClearLogs();
            }
        }


        public void AddLog(IEventBusLogable bus, string log, BusLogType logType)
        {
            _eventBusLogs.Add(new LogEntry(log, logType, bus));
            Repaint();
        }


        public void ClearLogs()
        {
            _eventBusLogs.Clear();
            Repaint();
        }


        private void AddEventBusToSelection(int index)
        {
            _eventBusMask |= 1 << (index);
        }


        private void HandleEventBusChange(IEventBusLogable bus, bool created)
        {
            if (created)
            {
                _eventBuses.Add(bus);
                AddEventBusToSelection(_eventBuses.IndexOf(bus));
            }
            else
            {
                _eventBuses.Remove(bus);
            }
        }


        ~EventBusLogWindow()
        {
            EventBusAggregator.OnLog -= AddLog;
            EventBusAggregator.OnNewEventBus -= HandleEventBusChange;
        }


        private static Texture2D MakeColorTexture(Color color)
        {
            Texture2D texture = new(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}