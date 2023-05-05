using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using Debug = MRDebug;

public class DebugMenu : MonoBehaviour
{

    [Header("Buttons:")]
    [SerializeField] Interactable _upPageBtn;
    [SerializeField] Interactable _downPageBtn;
    [SerializeField] Interactable _infoFilterBtn;
    [SerializeField] Interactable _warningFilterBtn;
    [SerializeField] Interactable _exceptionFilterBtn;
    [SerializeField] Interactable _errorFilterBtn;
    [SerializeField] Interactable _fatalFilterBtn;
    [SerializeField] Interactable _filterToggleBtn;

    [Header("Mesh:")]
    [SerializeField] MeshRenderer _infoFilterMesh;
    [SerializeField] MeshRenderer _warningFilterMesh;
    [SerializeField] MeshRenderer _exceptionFilterMesh;
    [SerializeField] MeshRenderer _errorFilterMesh;
    [SerializeField] MeshRenderer _fatalFilterMesh;
    [SerializeField] MeshRenderer _filterToggleMesh;

    [Header("Texts:")]
    [SerializeField] TextMeshPro _console;
    [SerializeField] TextMeshPro _pagination;

    [Header("Dettached Windows:")]
    [SerializeField] GameObject _filtersWindow;

    bool _filterInfo = true;
    bool _filterWarning = false;
    bool _filterException = false;
    bool _filterError = false;
    bool _filterFatal = false;

    bool _isFiltersVisible = false;
    

    private void Start() {
        _console.ForceMeshUpdate(true);

        MRDebug.Console = _console;

        _console.text = "";
        UpdateConsole();

        _upPageBtn.OnClick.AddListener(() => {
            _console.pageToDisplay = _console.pageToDisplay >= _console.textInfo.pageCount ? 1 : _console.pageToDisplay + 1;
            UpdatePagination();

        });

        _downPageBtn.OnClick.AddListener(() => {
            _console.pageToDisplay = _console.pageToDisplay <= 1 ? _console.textInfo.pageCount : _console.pageToDisplay - 1;
            UpdatePagination();

        });

        _infoFilterBtn.OnClick.AddListener(() => {
            _filterInfo = !_filterInfo;
            _console.pageToDisplay = 1;
            _console.text = "";

            if (_filterInfo)
                _infoFilterMesh.material = UIManager.Instance.GetCircleButtonMaterial("Info").Value.ActiveMaterial;
            else
                _infoFilterMesh.material = UIManager.Instance.GetCircleButtonMaterial("Info").Value.InactiveMaterial;

            UpdateConsole();

        });

        _warningFilterBtn.OnClick.AddListener(() => {
            _filterWarning = !_filterWarning;
            _console.pageToDisplay = 1;
            _console.text = "";

            if (_filterWarning)
                _warningFilterMesh.material = UIManager.Instance.GetCircleButtonMaterial("Warning").Value.ActiveMaterial;
            else
                _warningFilterMesh.material = UIManager.Instance.GetCircleButtonMaterial("Warning").Value.InactiveMaterial;

            UpdateConsole();

        });

        _exceptionFilterBtn.OnClick.AddListener(() => {
            _filterException = !_filterException;
            _console.pageToDisplay = 1;
            _console.text = "";

            if (_filterException)
                _exceptionFilterMesh.material = UIManager.Instance.GetCircleButtonMaterial("Exception").Value.ActiveMaterial;
            else
                _exceptionFilterMesh.material = UIManager.Instance.GetCircleButtonMaterial("Exception").Value.InactiveMaterial;

            UpdateConsole();

        });

        _errorFilterBtn.OnClick.AddListener(() => {
            _filterError = !_filterError;
            _console.pageToDisplay = 1;
            _console.text = "";

            if (_filterError)
                _errorFilterMesh.material = UIManager.Instance.GetCircleButtonMaterial("Error").Value.ActiveMaterial;
            else
                _errorFilterMesh.material = UIManager.Instance.GetCircleButtonMaterial("Error").Value.InactiveMaterial;

            UpdateConsole();

        });

        _fatalFilterBtn.OnClick.AddListener(() => {
            _filterFatal = !_filterFatal;
            _console.pageToDisplay = 1;
            _console.text = "";

            if (_filterFatal)
                _fatalFilterMesh.material = UIManager.Instance.GetCircleButtonMaterial("Fatal").Value.ActiveMaterial;
            else
                _fatalFilterMesh.material = UIManager.Instance.GetCircleButtonMaterial("Fatal").Value.InactiveMaterial;

            UpdateConsole();

        });

        _filterToggleBtn.OnClick.AddListener(() => {
            _isFiltersVisible = !_isFiltersVisible;

            _filtersWindow.SetActive(_isFiltersVisible);

            if (_isFiltersVisible)
                _filterToggleMesh.material = UIManager.Instance.GetCircleButtonMaterial("Filter").Value.InactiveMaterial;
            else
                _filterToggleMesh.material = UIManager.Instance.GetCircleButtonMaterial("Filter").Value.ActiveMaterial;


        });

    }

    private void UpdatePagination() { _pagination.text = _console.pageToDisplay + " / " + _console.textInfo.pageCount; }

    private void OnEnable() {
        UIManager.Instance.HomeMenu.gameObject.SetActive(false);
        UpdateConsole();

    }

    public void UpdateConsole() {
        if (_console == null)
            return;
        _console.text = "";
        foreach (MRDebug.AppLog log in MRDebug.GetLog(_filterInfo, _filterWarning, _filterException, _filterError, _filterFatal)) {
            _console.text = _console.text + log.info;

        }
        UpdatePagination();

    }
}
