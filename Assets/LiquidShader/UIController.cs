using System;
using System.Collections.Generic;
using System.Reflection;
// using PlasticPipe.PlasticProtocol.Messages;
using UnityEngine;
using Utils;
using Object = UnityEngine.Object;

namespace LiquidShader {
public class MyFieldInfo {
    public Object target;
    public Action onChanged;
}

public class ChangeDetector {
    Dictionary<string, object> lastValueByName = new Dictionary<string, object>();
    Dictionary<string, MyFieldInfo> fieldInfoByName = new Dictionary<string, MyFieldInfo>();
    public void Register(Object target, string fieldName, Action onChanged) {
        var fieldInfo = new MyFieldInfo() {
            target = target, onChanged = onChanged
        };
        fieldInfoByName[fieldName] = fieldInfo;
        lastValueByName[fieldName] = GetValue(target, fieldName);
    }

    public void CheckChanges() {
        foreach (var (fieldName, fieldInfo) in fieldInfoByName) {
            var lastValue = lastValueByName[fieldName];
            var currentValue = GetValue(fieldInfo.target, fieldName);
            // Debug.Log($"fieldName {fieldName} {lastValue.GetType()} lastValue {lastValue} currentValue {currentValue} {currentValue.GetType()}");
            if (!currentValue.Equals(lastValue)) {
                lastValueByName[fieldName] = currentValue;
                fieldInfo.onChanged();
            }
        }
    }

    object GetValue(object target, string fieldName) {
        var fieldInfos = target.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        for (var i = 0; i < fieldInfos.Length; i++) {
            var fieldInfo = fieldInfos[i];
            if (fieldInfo.Name == fieldName) {
                var targetValue = fieldInfo.GetValue(target);
                return targetValue;
            }
        }
        throw new Exception($"couldnt find name {fieldName}");
    }
}

public class UIController : MonoBehaviour {
    [SerializeField] bool showVelocityDrawing = false;
    [SerializeField] bool showVelocityPersist = false;
    [SerializeField] bool showColorDrawing = false;
    [SerializeField] bool showColorPersist = false;
    [SerializeField] bool showClearDivergence = false;
    [SerializeField] bool showDrawingRadius = false;

    [SerializeField] bool showDesignButton = false;
    [SerializeField] bool showCellActiveButton = false;
    [SerializeField] bool showColorSrcButton = false;
    [SerializeField] bool showColorButton = false;
    [SerializeField] bool showMovingTexButton = false;
    [SerializeField] bool showColorHsvButton = false;
    [SerializeField] bool showDivergenceButton = false;

    [SerializeField] GameObjects gameObjects;

    [Serializable]
    struct GameObjects {
        public GameObject velocityDrawingObject;
        public GameObject velocityPersistObject;
        public GameObject colorDrawingObject;
        public GameObject colorPersistObject;
        public GameObject drawingRadiusObject;
        public GameObject clearDivergenceObject;

        public GameObject designButton;
        public GameObject cellActiveButton;
        public GameObject colorSrcButton;
        public GameObject colorButton;
        public GameObject movingTexButton;
        public GameObject colorHsvButton;
        public GameObject divergenceButton;
    }

    ChangeDetector _changeDetector;

    void OnEnable() {
        _changeDetector = new ChangeDetector();
        _changeDetector.Register(this, nameof(showVelocityDrawing), ChangeDetected);
        _changeDetector.Register(this, nameof(showColorDrawing), ChangeDetected);
        _changeDetector.Register(this, nameof(showClearDivergence), ChangeDetected);
        var fields = new string[] {
            nameof(showVelocityPersist), nameof(showColorPersist),
            nameof(showDrawingRadius),
            nameof(showDesignButton),
            nameof(showCellActiveButton),
            nameof(showColorSrcButton),
            nameof(showColorButton),
            nameof(showMovingTexButton),
            nameof(showColorHsvButton),
            nameof(showDivergenceButton)
        };
        foreach (var fieldName in fields) {
            _changeDetector.Register(this, fieldName, ChangeDetected);
        }
        ChangeDetected();
    }

    void ChangeDetected() {
        Debug.Log("change detected");
        gameObjects.velocityDrawingObject.SetActive(showVelocityDrawing);
        gameObjects.colorDrawingObject.SetActive(showColorDrawing);
        gameObjects.clearDivergenceObject.SetActive(showClearDivergence);
        gameObjects.velocityPersistObject.SetActive(showVelocityPersist);
        gameObjects.colorPersistObject.SetActive(showColorPersist);
        gameObjects.drawingRadiusObject.SetActive(showDrawingRadius);

        gameObjects.designButton.SetActive(showDesignButton);
        gameObjects.cellActiveButton.SetActive(showCellActiveButton);
        gameObjects.colorSrcButton.SetActive(showColorSrcButton);
        gameObjects.colorButton.SetActive(showColorButton);
        gameObjects.movingTexButton.SetActive(showMovingTexButton);
        gameObjects.colorHsvButton.SetActive(showColorHsvButton);
        gameObjects.divergenceButton.SetActive(showDivergenceButton);
    }

    void Update() {
        if (_changeDetector == null) return;
        _changeDetector.CheckChanges();
    }
}
}
