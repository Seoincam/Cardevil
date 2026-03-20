using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Database
{
    public class DBInitializerWindow : EditorWindow
    {
        private const string UssPath = "Assets/Database/Editor/MDBLoaderEditor.uss";
        private const string RowHeightPrefKey = "Database.DBInitializerWindow.RowHeight";
        private const string LanguagePrefKey = "Database.DBInitializerWindow.Language";

        private static readonly HashSet<string> ReadOnlyProperties = new HashSet<string>
        {
            "m_Script",
            "thisDirPath",
            "dataDirFullPath",
            "targetClassFullPath",
            "targetFullPath",
            "downloadStateDirFullPath"
        };

        private DBInitializerSO _initializer;
        private ObjectField _initializerField;
        private DropdownField _languageDropdown;
        private VisualElement _emptyState;
        private ScrollView _contentRoot;

        private readonly List<SheetPreview> _allSheets = new List<SheetPreview>();
        private readonly List<SheetPreview> _filteredSheets = new List<SheetPreview>();
        private readonly List<RowPreview> _visibleRows = new List<RowPreview>();

        private PreviewSource _previewSource = PreviewSource.DataFrame;
        private UiLanguage _language = UiLanguage.Korean;
        private string _selectedSheetName;
        private string _searchText = string.Empty;
        private string _sheetSearchText = string.Empty;
        private int _sortColumnIndex = -1;
        private bool _sortDescending;
        private int _rowHeight = 28;
        private DateTime _lastPreviewRefreshUtc = DateTime.MinValue;
        private DBInitializerDownloadState _downloadState = DBInitializerDownloadState.CreateEmpty();

        private Label _pathSummaryLabel;
        private Label _sheetSummaryLabel;
        private Label _previewTitleLabel;
        private Label _previewMetaLabel;
        private Label _sortStatusLabel;
        private Label _sheetCaptionLabel;
        private Label _emptyStateLabel;
        private Label _previewEmptyStateLabel;
        private ToolbarSearchField _sheetSearchField;
        private ToolbarSearchField _rowSearchField;
        private DropdownField _sortColumnDropdown;
        private Button _sortDirectionButton;
        private SliderInt _rowHeightSlider;
        private ListView _sheetListView;
        private MultiColumnListView _tableView;
        private Label _sheetSourceValueLabel;
        private Label _sheetRowsValueLabel;
        private Label _sheetColumnsValueLabel;
        private Label _sheetFileValueLabel;
        private Label _previewRefreshValueLabel;
        private Label _fullDownloadValueLabel;
        private Label _sheetDownloadValueLabel;
        private VisualElement _previewEmptyState;
        private VisualElement _rowListContainer;
        private TextField _rowDetailField;

        [MenuItem("Tools/Database/Initializer Window")]
        public static void OpenWindow()
        {
            OpenWindow(null);
        }

        public static void OpenWindow(DBInitializerSO initializer)
        {
            var window = GetWindow<DBInitializerWindow>(DBInitializerWindowLocalization.Get(ReadLanguagePref(), "window.title"));
            window.minSize = new Vector2(1080f, 740f);

            if (initializer != null)
            {
                window.SetInitializer(initializer, true);
            }
            else if (window.rootVisualElement.childCount > 0)
            {
                window.TryAssignDefaultInitializer();
                window.Refresh();
            }
        }

        public void CreateGUI()
        {
            _rowHeight = EditorPrefs.GetInt(RowHeightPrefKey, 28);
            _language = ReadLanguagePref();
            BuildUi();
            TryAssignDefaultInitializer();
            Refresh();
        }

        private void OnSelectionChange()
        {
            if (Selection.activeObject is DBInitializerSO initializer)
                SetInitializer(initializer);
        }

        private void UseCurrentSelection()
        {
            if (TryGetSelectedInitializer(out var selectedInitializer))
            {
                SetInitializer(selectedInitializer, true);
                return;
            }

            string message = _language == UiLanguage.Korean
                ? "Project 창에서 DBInitializerSO 에셋을 먼저 선택하세요."
                : "Select a DBInitializerSO asset in the Project window first.";
            EditorUtility.DisplayDialog(L("dialog.title"), message, L("dialog.ok"));
        }

        private string L(string key)
        {
            return DBInitializerWindowLocalization.Get(_language, key);
        }

        private string LF(string key, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, L(key), args);
        }

        private static UiLanguage ReadLanguagePref()
        {
            return (UiLanguage)EditorPrefs.GetInt(LanguagePrefKey, (int)UiLanguage.Korean);
        }

        private void BuildUi()
        {
            rootVisualElement.Clear();
            titleContent = new GUIContent(L("window.title"));

            if (AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath) is { } styleSheet)
                rootVisualElement.styleSheets.Add(styleSheet);

            var windowRoot = new VisualElement();
            windowRoot.AddToClassList("db-window");

            var header = new VisualElement();
            header.AddToClassList("db-header");

            var headerText = new VisualElement();
            headerText.AddToClassList("db-header-text");

            var kicker = new Label(L("header.kicker"));
            kicker.AddToClassList("db-kicker");
            headerText.Add(kicker);

            var title = new Label(L("window.title"));
            title.AddToClassList("db-title");
            headerText.Add(title);

            var subtitle = new Label(L("header.subtitle"));
            subtitle.AddToClassList("db-subtitle");
            headerText.Add(subtitle);

            header.Add(headerText);

            var headerActions = new VisualElement();
            headerActions.AddToClassList("db-toolbar-actions");
            headerActions.Add(CreateLanguageDropdown());
            headerActions.Add(CreateToolbarButton(L("button.useSelection"), L("tooltip.useSelection"), UseCurrentSelection, false));
            headerActions.Add(CreateToolbarButton(L("button.reloadPreview"), L("tooltip.reloadPreview"), ReloadPreviewData, true));
            header.Add(headerActions);

            windowRoot.Add(header);

            var toolbar = new VisualElement();
            toolbar.AddToClassList("db-toolbar");

            _initializerField = new ObjectField(L("field.initializer"))
            {
                objectType = typeof(DBInitializerSO),
                allowSceneObjects = false,
                value = _initializer
            };
            _initializerField.AddToClassList("db-object-field");
            _initializerField.RegisterValueChangedCallback(evt => SetInitializer(evt.newValue as DBInitializerSO, true));
            toolbar.Add(_initializerField);

            toolbar.Add(CreateToolbarButton(L("button.pingAsset"), L("tooltip.pingAsset"), PingInitializer, false));
            toolbar.Add(CreateToolbarButton(L("button.openTargetFolder"), L("tooltip.openTargetFolder"), OpenTargetFolder, false));
            toolbar.Add(CreateToolbarButton(L("button.openRawFolder"), L("tooltip.openRawFolder"), OpenRawFolder, false));

            windowRoot.Add(toolbar);

            _emptyState = CreateInlineMessage(L("message.selectInitializer"), out _emptyStateLabel);
            windowRoot.Add(_emptyState);

            _contentRoot = new ScrollView();
            _contentRoot.AddToClassList("db-scroll");
            windowRoot.Add(_contentRoot);

            rootVisualElement.Add(windowRoot);
        }

        private void TryAssignDefaultInitializer()
        {
            if (_initializer != null)
                return;

            if (TryGetSelectedInitializer(out var selectedInitializer))
            {
                _initializer = selectedInitializer;
            }
            else
            {
                string[] guids = AssetDatabase.FindAssets("t:DBInitializerSO");
                if (guids.Length > 0)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _initializer = AssetDatabase.LoadAssetAtPath<DBInitializerSO>(assetPath);
                }
            }

            if (_initializerField != null)
            {
                _initializerField.SetValueWithoutNotify(_initializer);
            }
        }

        private static bool TryGetSelectedInitializer(out DBInitializerSO initializer)
        {
            initializer = Selection.activeObject as DBInitializerSO;
            if (initializer != null)
            {
                return true;
            }

            var selectedAssets = Selection.GetFiltered<DBInitializerSO>(SelectionMode.Assets);
            if (selectedAssets != null && selectedAssets.Length > 0)
            {
                initializer = selectedAssets[0];
                return true;
            }

            return false;
        }

        private DropdownField CreateLanguageDropdown()
        {
            _languageDropdown = new DropdownField(
                new List<string> { L("language.korean"), L("language.english") },
                _language == UiLanguage.Korean ? 0 : 1)
            {
                tooltip = L("tooltip.language")
            };
            _languageDropdown.AddToClassList("db-language-field");
            _languageDropdown.RegisterValueChangedCallback(evt =>
            {
                UiLanguage nextLanguage = evt.newValue == L("language.korean") ? UiLanguage.Korean : UiLanguage.English;
                if (nextLanguage == _language)
                    return;

                _language = nextLanguage;
                EditorPrefs.SetInt(LanguagePrefKey, (int)_language);
                BuildUi();
                TryAssignDefaultInitializer();
                Refresh();
            });

            return _languageDropdown;
        }

        private void SetInitializer(DBInitializerSO initializer, bool forceRefresh = false)
        {
            if (!forceRefresh && ReferenceEquals(_initializer, initializer))
                return;

            _initializer = initializer;
            _downloadState = DBInitializerDownloadStateStore.Load(_initializer);

            if (_initializerField != null)
                _initializerField.SetValueWithoutNotify(_initializer);

            Refresh();
        }

        private void Refresh()
        {
            if (_contentRoot == null || _emptyState == null)
                return;

            _contentRoot.Clear();
            _emptyState.style.display = _initializer == null ? DisplayStyle.Flex : DisplayStyle.None;

            if (_initializer == null)
                return;

            _contentRoot.Add(BuildPathSection());
            _contentRoot.Add(BuildPropertySection());
            _contentRoot.Add(BuildActionSection());
            _contentRoot.Add(BuildPreviewSection());

            ReloadPreviewData();
        }

        private VisualElement BuildPathSection()
        {
            var section = CreateSection(L("section.paths.title"), L("section.paths.desc"));

            var grid = new VisualElement();
            grid.AddToClassList("db-path-grid");
            grid.Add(CreatePathCard(L("path.raw"), _initializer.DataDirPath));
            grid.Add(CreatePathCard(L("path.generated"), _initializer.TargetClassPath));
            grid.Add(CreatePathCard(L("path.output"), _initializer.TargetDirPath));
            grid.Add(CreatePathCard(_language == UiLanguage.Korean ? "상태 파일" : "State Files", _initializer.DownloadStateDirPath));
            section.Add(grid);

            _pathSummaryLabel = new Label();
            _pathSummaryLabel.AddToClassList("db-section-meta");
            section.Add(_pathSummaryLabel);

            UpdatePathSummary();
            return section;
        }

        private VisualElement BuildPropertySection()
        {
            var section = CreateSection(L("section.settings.title"), L("section.settings.desc"));

            var serializedObject = new SerializedObject(_initializer);
            var iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                var property = iterator.Copy();
                var field = new PropertyField(property);
                field.SetEnabled(!ReadOnlyProperties.Contains(property.propertyPath));
                field.AddToClassList("db-property");
                section.Add(field);
                enterChildren = false;
            }

            section.Bind(serializedObject);
            return section;
        }

        private VisualElement BuildActionSection()
        {
            var section = CreateSection(L("section.actions.title"), L("section.actions.desc"));

            var grid = new VisualElement();
            grid.AddToClassList("db-actions");

            grid.Add(CreateActionButton(L("button.generateXlsx"), L("tooltip.generateXlsx"), () => _initializer.LoadAll(_initializer.xlsxExtension), false));
            grid.Add(CreateActionButton(L("button.generateJson"), L("tooltip.generateJson"), () => _initializer.LoadAll(_initializer.jsonExtension), false));
            grid.Add(CreateActionButton(L("button.downloadGoogleSheet"), L("tooltip.downloadGoogleSheet"), DownloadGoogleSheet, true));
            grid.Add(CreateActionButton(L("button.copyAppsScript"), L("tooltip.copyAppsScript"), CopyGoogleAppsScript, false));
            grid.Add(CreateActionButton(L("button.openCacheFolder"), L("tooltip.openCacheFolder"), () => EditorApplication.ExecuteMenuItem("Tools/Database/Open Cache Folder"), false));
            grid.Add(CreateActionButton(L("button.clearImageCache"), L("tooltip.clearImageCache"), () => EditorApplication.ExecuteMenuItem("Tools/Database/Clear Image Cache"), false));

            section.Add(grid);
            return section;
        }

        private VisualElement BuildPreviewSection()
        {
            var section = CreateSection(L("section.preview.title"), L("section.preview.desc"));
            section.AddToClassList("db-preview-section");

            var header = new VisualElement();
            header.AddToClassList("db-section-header");

            var titleGroup = new VisualElement();
            titleGroup.AddToClassList("db-title-group");

            _previewMetaLabel = new Label(L("preview.selectSource"));
            _previewMetaLabel.AddToClassList("db-section-meta");
            titleGroup.Add(_previewMetaLabel);
            header.Add(titleGroup);

            var sourceField = new PopupField<string>(
                L("field.source"),
                new List<string> { L("source.dataframe"), L("source.json") },
                _previewSource == PreviewSource.DataFrame ? 0 : 1);
            sourceField.AddToClassList("db-source-field");
            sourceField.tooltip = L("tooltip.source");
            sourceField.RegisterValueChangedCallback(evt =>
            {
                _previewSource = evt.newValue == L("source.json")
                    ? PreviewSource.Json
                    : PreviewSource.DataFrame;
                _selectedSheetName = null;
                _sortColumnIndex = -1;
                _sortDescending = false;
                _searchText = string.Empty;
                if (_rowSearchField != null)
                    _rowSearchField.SetValueWithoutNotify(string.Empty);
                ReloadPreviewData();
            });
            header.Add(sourceField);

            section.Add(header);

            var previewShell = new VisualElement();
            previewShell.AddToClassList("db-preview-shell");

            previewShell.Add(BuildSheetSidebar());
            previewShell.Add(BuildPreviewPane());

            section.Add(previewShell);
            return section;
        }

        private VisualElement BuildSheetSidebar()
        {
            var sidebar = new VisualElement();
            sidebar.AddToClassList("db-sidebar");

            var sidebarHeader = new VisualElement();
            sidebarHeader.AddToClassList("db-sidebar-header");

            var sidebarTitle = new Label(L("sidebar.sheets"));
            sidebarTitle.AddToClassList("db-sidebar-title");
            sidebarHeader.Add(sidebarTitle);

            _sheetCaptionLabel = new Label(L("sidebar.sheetsDesc"));
            _sheetCaptionLabel.AddToClassList("db-sidebar-caption");
            sidebarHeader.Add(_sheetCaptionLabel);

            _sheetSummaryLabel = new Label(LF("sidebar.sheetCount", 0));
            _sheetSummaryLabel.AddToClassList("db-sidebar-title");
            sidebarHeader.Add(_sheetSummaryLabel);

            sidebar.Add(sidebarHeader);

            _sheetSearchField = new ToolbarSearchField();
            _sheetSearchField.AddToClassList("db-sidebar-search");
            _sheetSearchField.tooltip = L("tooltip.sheetSearch");
            _sheetSearchField.RegisterValueChangedCallback(evt =>
            {
                _sheetSearchText = evt.newValue ?? string.Empty;
                ApplySheetFilterAndSelection();
            });
            sidebar.Add(_sheetSearchField);

            _sheetListView = new ListView
            {
                itemsSource = _filteredSheets,
                selectionType = SelectionType.Single,
                fixedItemHeight = 56
            };
            _sheetListView.AddToClassList("db-sheet-list");
            _sheetListView.makeItem = CreateSheetListItem;
            _sheetListView.bindItem = BindSheetListItem;
            _sheetListView.selectionChanged += _ =>
            {
                if (_sheetListView.selectedItem is SheetPreview selectedSheet)
                {
                    _selectedSheetName = selectedSheet.Name;
                    RefreshPreviewPane();
                }
            };
            sidebar.Add(_sheetListView);

            return sidebar;
        }

        private VisualElement BuildPreviewPane()
        {
            var previewPane = new VisualElement();
            previewPane.AddToClassList("db-preview-pane");

            var topBar = new VisualElement();
            topBar.AddToClassList("db-preview-toolbar");

            var titleBlock = new VisualElement();
            titleBlock.AddToClassList("db-preview-title-block");

            _previewTitleLabel = new Label(L("preview.noSheet"));
            _previewTitleLabel.AddToClassList("db-preview-title");
            titleBlock.Add(_previewTitleLabel);

            _sortStatusLabel = new Label(L("sort.original"));
            _sortStatusLabel.AddToClassList("db-preview-caption");
            titleBlock.Add(_sortStatusLabel);

            topBar.Add(titleBlock);

            var actionRow = new VisualElement();
            actionRow.AddToClassList("db-preview-actions");

            _sortColumnDropdown = new DropdownField(L("field.sort"), new List<string> { L("sort.original") }, 0);
            _sortColumnDropdown.AddToClassList("db-sort-field");
            _sortColumnDropdown.tooltip = L("tooltip.sortColumn");
            _sortColumnDropdown.RegisterValueChangedCallback(_ => OnSortColumnChanged());
            actionRow.Add(_sortColumnDropdown);

            _sortDirectionButton = CreateToolbarButton(L("sort.asc"), L("tooltip.sortDirection"), ToggleSortDirection, false);
            _sortDirectionButton.AddToClassList("db-sort-direction");
            actionRow.Add(_sortDirectionButton);

            _rowHeightSlider = new SliderInt(L("field.rowHeight"), 24, 52)
            {
                value = _rowHeight
            };
            _rowHeightSlider.AddToClassList("db-row-height-slider");
            _rowHeightSlider.tooltip = L("tooltip.rowHeight");
            _rowHeightSlider.RegisterValueChangedCallback(evt => UpdateRowHeight(evt.newValue));
            actionRow.Add(_rowHeightSlider);

            _rowSearchField = new ToolbarSearchField();
            _rowSearchField.AddToClassList("db-search");
            _rowSearchField.tooltip = L("tooltip.rowSearch");
            _rowSearchField.RegisterValueChangedCallback(evt =>
            {
                _searchText = evt.newValue ?? string.Empty;
                ApplyRowTransforms();
            });
            actionRow.Add(_rowSearchField);

            actionRow.Add(CreateToolbarButton(L("button.clearSearch"), L("tooltip.clearSearch"), ClearRowSearch, false));
            actionRow.Add(CreateToolbarButton(L("button.resetSort"), L("tooltip.resetSort"), ResetSort, false));
            actionRow.Add(CreateToolbarButton(L("button.refreshSheetFromGoogle"), L("tooltip.refreshSheetFromGoogle"), RefreshSelectedSheetFromGoogle, true));
            actionRow.Add(CreateToolbarButton(L("button.openFile"), L("tooltip.openFile"), OpenSelectedFile, false));
            actionRow.Add(CreateToolbarButton(L("button.copyRow"), L("tooltip.copyRow"), CopySelectedRow, false));

            topBar.Add(actionRow);
            previewPane.Add(topBar);

            var summaryGrid = new VisualElement();
            summaryGrid.AddToClassList("db-sheet-summary-grid");
            summaryGrid.Add(CreateSummaryCard(L("summary.source"), out _sheetSourceValueLabel));
            summaryGrid.Add(CreateSummaryCard(L("summary.rows"), out _sheetRowsValueLabel));
            summaryGrid.Add(CreateSummaryCard(L("summary.columns"), out _sheetColumnsValueLabel));
            summaryGrid.Add(CreateSummaryCard(L("summary.file"), out _sheetFileValueLabel));
            summaryGrid.Add(CreateSummaryCard(_language == UiLanguage.Korean ? "마지막 새로고침" : "Last Refresh", out _previewRefreshValueLabel));
            summaryGrid.Add(CreateSummaryCard(_language == UiLanguage.Korean ? "마지막 전체 다운로드" : "Last Full Download", out _fullDownloadValueLabel));
            summaryGrid.Add(CreateSummaryCard(_language == UiLanguage.Korean ? "선택 시트 다운로드" : "Selected Sheet Download", out _sheetDownloadValueLabel));
            previewPane.Add(summaryGrid);

            _previewEmptyState = CreateInlineMessage(L("message.selectSheet"), out _previewEmptyStateLabel);
            previewPane.Add(_previewEmptyState);

            _rowListContainer = new VisualElement();
            _rowListContainer.AddToClassList("db-table-list-container");
            previewPane.Add(_rowListContainer);

            var detailSection = new VisualElement();
            detailSection.AddToClassList("db-detail-section");

            var detailHeader = new VisualElement();
            detailHeader.AddToClassList("db-detail-header");

            var detailTitle = new Label(L("detail.selectedRow"));
            detailTitle.AddToClassList("db-detail-title");
            detailHeader.Add(detailTitle);

            var detailActions = new VisualElement();
            detailActions.AddToClassList("db-row-detail-actions");
            detailActions.Add(CreateToolbarButton(L("button.copyDetail"), L("tooltip.copyDetail"), CopySelectedRow, false));
            detailHeader.Add(detailActions);

            detailSection.Add(detailHeader);

            _rowDetailField = new TextField
            {
                multiline = true,
                isReadOnly = true
            };
            _rowDetailField.AddToClassList("db-row-detail");
            detailSection.Add(_rowDetailField);

            previewPane.Add(detailSection);
            return previewPane;
        }

        private VisualElement CreateSection(string titleText, string description)
        {
            var section = new VisualElement();
            section.AddToClassList("db-section");

            var header = new VisualElement();
            header.AddToClassList("db-section-stack");

            var title = new Label(titleText);
            title.AddToClassList("db-section-title");
            header.Add(title);

            if (!string.IsNullOrWhiteSpace(description))
            {
                var desc = new Label(description);
                desc.AddToClassList("db-section-description");
                header.Add(desc);
            }

            section.Add(header);
            return section;
        }

        private VisualElement CreatePathCard(string label, string value)
        {
            var card = new VisualElement();
            card.AddToClassList("db-path-card");

            var title = new Label(label);
            title.AddToClassList("db-path-label");
            title.tooltip = value;
            card.Add(title);

            var path = new Label(value);
            path.AddToClassList("db-path-value");
            path.tooltip = value;
            card.Add(path);

            return card;
        }

        private VisualElement CreateSummaryCard(string label, out Label valueLabel)
        {
            var card = new VisualElement();
            card.AddToClassList("db-sheet-summary-card");

            var labelElement = new Label(label);
            labelElement.AddToClassList("db-sheet-summary-label");
            card.Add(labelElement);

            valueLabel = new Label("-");
            valueLabel.AddToClassList("db-sheet-summary-value");
            valueLabel.tooltip = label;
            card.Add(valueLabel);

            return card;
        }

        private VisualElement CreateInlineMessage(string text, out Label label)
        {
            var container = new VisualElement();
            container.AddToClassList("db-inline-message");

            label = new Label(text);
            label.AddToClassList("db-inline-message-text");
            label.tooltip = text;
            container.Add(label);

            return container;
        }

        private Button CreateToolbarButton(string text, string tooltip, Action onClick, bool emphasized)
        {
            var button = new Button(() =>
            {
                try
                {
                    onClick?.Invoke();
                }
                catch (Exception ex)
                {
                    EditorUtility.DisplayDialog(L("dialog.title"), ex.Message, L("dialog.ok"));
                }
            })
            {
                text = text
            };

            button.tooltip = tooltip;
            button.AddToClassList(emphasized ? "db-toolbar-button-primary" : "db-toolbar-button");
            return button;
        }

        private Button CreateActionButton(string text, string tooltip, Action onClick, bool primary)
        {
            var button = new Button(() =>
            {
                try
                {
                    onClick?.Invoke();
                    AssetDatabase.Refresh();
                    ReloadPreviewData();
                }
                catch (Exception ex)
                {
                    EditorUtility.DisplayDialog(L("dialog.title"), ex.Message, L("dialog.ok"));
                }
            })
            {
                text = text
            };

            button.tooltip = tooltip;
            button.AddToClassList(primary ? "db-action-button-primary" : "db-action-button");
            return button;
        }

        private VisualElement CreateSheetListItem()
        {
            var container = new VisualElement();
            container.AddToClassList("db-sheet-item");

            var name = new Label();
            name.name = "sheet-name";
            name.AddToClassList("db-sheet-item-name");
            container.Add(name);

            var meta = new Label();
            meta.name = "sheet-meta";
            meta.AddToClassList("db-sheet-item-meta");
            container.Add(meta);

            return container;
        }

        private void BindSheetListItem(VisualElement element, int index)
        {
            if (index < 0 || index >= _filteredSheets.Count)
                return;

            SheetPreview sheet = _filteredSheets[index];
            if (element.Q<Label>("sheet-name") is { } nameLabel)
            {
                nameLabel.text = sheet.Name;
                nameLabel.tooltip = sheet.Name;
            }
            if (element.Q<Label>("sheet-meta") is { } metaLabel)
            {
                metaLabel.text = LF("sheet.meta", sheet.Rows.Count, sheet.Columns.Count);
                metaLabel.tooltip = LF("sheet.meta", sheet.Rows.Count, sheet.Columns.Count);
            }

            element.tooltip = $"{sheet.Name}\n{sheet.FilePath}";
        }

        private void ReloadPreviewData()
        {
            _allSheets.Clear();
            _filteredSheets.Clear();
            _visibleRows.Clear();
            _downloadState = DBInitializerDownloadStateStore.Load(_initializer);

            if (_sheetListView != null)
            {
                _sheetListView.itemsSource = _filteredSheets;
                _sheetListView.Rebuild();
            }

            if (_initializer == null)
            {
                RefreshPreviewPane();
                return;
            }

            UpdatePathSummary();

            string targetDirPath = _initializer.TargetDirPath;
            if (!Directory.Exists(targetDirPath))
            {
                _previewMetaLabel.text = LF("preview.targetDirMissing", targetDirPath);
                RefreshPreviewPane();
                return;
            }

            try
            {
                _allSheets.AddRange(_previewSource == PreviewSource.DataFrame
                    ? LoadDataFrameSheets(targetDirPath)
                    : LoadJsonSheets(targetDirPath));
                _lastPreviewRefreshUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _previewMetaLabel.text = LF("preview.loadFailed", ex.Message);
            }

            _allSheets.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));
            ApplySheetFilterAndSelection();
        }

        private void ApplySheetFilterAndSelection()
        {
            _filteredSheets.Clear();

            IEnumerable<SheetPreview> sheets = _allSheets;
            if (!string.IsNullOrWhiteSpace(_sheetSearchText))
            {
                sheets = sheets.Where(sheet =>
                    sheet.Name.IndexOf(_sheetSearchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            _filteredSheets.AddRange(sheets);
            UpdateSheetSummary();

            if (_sheetListView != null)
            {
                _sheetListView.itemsSource = _filteredSheets;
                _sheetListView.Rebuild();
            }

            if (_filteredSheets.Count == 0)
            {
                _selectedSheetName = null;
                if (_sheetListView != null)
                    _sheetListView.ClearSelection();
                RefreshPreviewPane();
                return;
            }

            int selectedIndex = _filteredSheets.FindIndex(sheet => sheet.Name == _selectedSheetName);
            if (selectedIndex < 0)
            {
                selectedIndex = 0;
                _selectedSheetName = _filteredSheets[0].Name;
            }

            _sheetListView?.SetSelection(selectedIndex);
            RefreshPreviewPane();
        }

        private List<SheetPreview> LoadDataFrameSheets(string targetDirPath)
        {
            var result = new List<SheetPreview>();
            foreach (string filePath in Directory.GetFiles(targetDirPath, "*.df"))
            {
                string json = File.ReadAllText(filePath, Encoding.UTF8);
                var dataFrame = JsonConvert.DeserializeObject<DataFrame>(json);
                if (dataFrame == null)
                    continue;

                int columnCount = dataFrame.MaxColumn;
                var columns = new List<ColumnPreview>(columnCount);
                for (int i = 0; i < columnCount; i++)
                {
                    columns.Add(new ColumnPreview(
                        dataFrame.GetVarName(i),
                        SafeGet(dataFrame.types, i),
                        SafeGet(dataFrame.comments, i)));
                }

                var rows = new List<RowPreview>();
                for (int rowIndex = 0; rowIndex < dataFrame.RowCount; rowIndex++)
                {
                    var rawRow = dataFrame.data[rowIndex] ?? Array.Empty<string>();
                    var cells = new List<string>(columnCount);
                    for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                        cells.Add(columnIndex < rawRow.Length ? rawRow[columnIndex] ?? string.Empty : string.Empty);

                    rows.Add(new RowPreview(rowIndex, cells, BuildSearchText(cells)));
                }

                result.Add(new SheetPreview(Path.GetFileNameWithoutExtension(filePath), filePath, columns, rows));
            }

            return result;
        }

        private List<SheetPreview> LoadJsonSheets(string targetDirPath)
        {
            var result = new List<SheetPreview>();
            foreach (string filePath in Directory.GetFiles(targetDirPath, "*.json"))
            {
                string json = File.ReadAllText(filePath, Encoding.UTF8);
                JToken root = JToken.Parse(json);

                List<ColumnPreview> columns = BuildJsonColumns(root);
                List<RowPreview> rows = BuildJsonRows(root, columns);

                result.Add(new SheetPreview(Path.GetFileNameWithoutExtension(filePath), filePath, columns, rows));
            }

            return result;
        }

        private static List<ColumnPreview> BuildJsonColumns(JToken root)
        {
            if (root is JObject objectRoot && objectRoot["data"] is JArray dataArray)
            {
                var comments = objectRoot["comment"] as JObject;
                var types = objectRoot["type"] as JObject;
                var orderedNames = new List<string>();
                var nameLookup = new HashSet<string>(StringComparer.Ordinal);

                if (types != null)
                {
                    foreach (JProperty property in types.Properties())
                    {
                        if (nameLookup.Add(property.Name))
                            orderedNames.Add(property.Name);
                    }
                }

                if (comments != null)
                {
                    foreach (JProperty property in comments.Properties())
                    {
                        if (nameLookup.Add(property.Name))
                            orderedNames.Add(property.Name);
                    }
                }

                foreach (JObject rowObject in dataArray.OfType<JObject>())
                {
                    foreach (JProperty property in rowObject.Properties())
                    {
                        if (nameLookup.Add(property.Name))
                            orderedNames.Add(property.Name);
                    }
                }

                return orderedNames
                    .Select(name => new ColumnPreview(
                        name,
                        types?[name]?.ToString() ?? "json",
                        comments?[name]?.ToString() ?? string.Empty))
                    .ToList();
            }

            var columnNames = new List<string>();
            var columnLookup = new HashSet<string>(StringComparer.Ordinal);

            foreach (JObject rowObject in EnumerateJsonObjects(root))
            {
                foreach (JProperty property in rowObject.Properties())
                {
                    if (columnLookup.Add(property.Name))
                        columnNames.Add(property.Name);
                }
            }

            return columnNames
                .Select(name => new ColumnPreview(name, "json", string.Empty))
                .ToList();
        }

        private static List<RowPreview> BuildJsonRows(JToken root, List<ColumnPreview> columns)
        {
            var rows = new List<RowPreview>();
            List<string> columnNames = columns.Select(column => column.Name).ToList();

            if (root is JObject objectRoot && objectRoot["data"] is JArray dataArray)
            {
                for (int rowIndex = 0; rowIndex < dataArray.Count; rowIndex++)
                {
                    var rowObject = dataArray[rowIndex] as JObject;
                    var cells = BuildRowCells(columnNames, rowObject);
                    rows.Add(new RowPreview(rowIndex, cells, BuildSearchText(cells)));
                }

                return rows;
            }

            if (root is JArray arrayRoot)
            {
                for (int rowIndex = 0; rowIndex < arrayRoot.Count; rowIndex++)
                {
                    var rowObject = arrayRoot[rowIndex] as JObject;
                    var cells = BuildRowCells(columnNames, rowObject);
                    rows.Add(new RowPreview(rowIndex, cells, BuildSearchText(cells)));
                }

                return rows;
            }

            if (root is JObject singleObject)
            {
                var cells = BuildRowCells(columnNames, singleObject);
                rows.Add(new RowPreview(0, cells, BuildSearchText(cells)));
            }

            return rows;
        }

        private void RefreshPreviewPane()
        {
            SheetPreview selectedSheet = GetSelectedSheet();
            bool hasSelection = selectedSheet != null;

            if (_previewEmptyState != null)
                _previewEmptyState.style.display = hasSelection ? DisplayStyle.None : DisplayStyle.Flex;
            _rowListContainer?.Clear();
            if (_rowDetailField != null)
                _rowDetailField.value = string.Empty;

            if (!hasSelection)
            {
                UpdateSortControls(null);
                SetSheetSummaryValues(null);
                if (_previewTitleLabel != null)
                    _previewTitleLabel.text = L("preview.noSheet");
                if (_previewMetaLabel != null)
                    _previewMetaLabel.text = BuildPreviewMeta(null);
                if (_sortStatusLabel != null)
                    _sortStatusLabel.text = L("sort.original");
                return;
            }

            _previewTitleLabel.text = selectedSheet.Name;
            _previewMetaLabel.text = BuildPreviewMeta(selectedSheet);
            SetSheetSummaryValues(selectedSheet);
            UpdateSortControls(selectedSheet);
            ApplyRowTransforms();
        }

        private void ApplyRowTransforms()
        {
            _visibleRows.Clear();
            SheetPreview selectedSheet = GetSelectedSheet();
            if (selectedSheet == null)
                return;

            IEnumerable<RowPreview> rows = selectedSheet.Rows;
            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                rows = rows.Where(row =>
                    row.SearchText.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            var rowList = rows.ToList();
            if (_sortColumnIndex >= 0 && _sortColumnIndex < selectedSheet.Columns.Count)
            {
                rowList.Sort((left, right) =>
                {
                    int comparison = CompareCellValues(
                        SafeGet(left.Cells, _sortColumnIndex),
                        SafeGet(right.Cells, _sortColumnIndex));
                    if (comparison == 0)
                        comparison = left.Index.CompareTo(right.Index);
                    return _sortDescending ? -comparison : comparison;
                });
            }

            _visibleRows.AddRange(rowList);
            if (_sortStatusLabel != null)
                _sortStatusLabel.text = BuildSortLabel(selectedSheet);
            BuildTable(selectedSheet);
        }

        private void BuildTable(SheetPreview sheet)
        {
            _rowListContainer.Clear();

            _tableView = new MultiColumnListView
            {
                itemsSource = _visibleRows,
                selectionType = SelectionType.Single,
                fixedItemHeight = _rowHeight,
                showAlternatingRowBackgrounds = AlternatingRowBackground.None,
                showBorder = true
            };
            _tableView.AddToClassList("db-table-list");
            _tableView.columns.Add(CreateIndexColumn());

            for (int i = 0; i < sheet.Columns.Count; i++)
                _tableView.columns.Add(CreateDataColumn(sheet, i));

            _tableView.selectionChanged += selection =>
            {
                RowPreview selectedRow = selection.OfType<RowPreview>().FirstOrDefault();
                _rowDetailField.value = selectedRow == null ? string.Empty : FormatRowDetail(sheet, selectedRow);
            };

            _rowListContainer.Add(_tableView);
            _tableView.schedule.Execute(() => ApplyColumnHeaderTooltips(sheet));
        }

        private Column CreateIndexColumn()
        {
            return new Column
            {
                title = L("table.index"),
                width = 52,
                minWidth = 52,
                stretchable = false,
                sortable = false,
                makeCell = () =>
                {
                    var label = new Label();
                    label.AddToClassList("db-table-index-cell");
                    return label;
                },
                bindCell = (element, rowIndex) =>
                {
                    if (element is Label label && rowIndex >= 0 && rowIndex < _visibleRows.Count)
                    {
                        label.text = (_visibleRows[rowIndex].Index + 1).ToString(CultureInfo.InvariantCulture);
                        label.tooltip = LF("tooltip.index", _visibleRows[rowIndex].Index + 1);
                    }
                }
            };
        }

        private Column CreateDataColumn(SheetPreview sheet, int columnIndex)
        {
            ColumnPreview column = sheet.Columns[columnIndex];
            return new Column
            {
                title = BuildColumnTitle(column, columnIndex),
                minWidth = 120,
                width = Mathf.Clamp(column.Name.Length * 14 + 48, 140, 260),
                stretchable = true,
                sortable = false,
                makeCell = () =>
                {
                    var label = new Label();
                    label.AddToClassList("db-table-cell");
                    return label;
                },
                bindCell = (element, rowIndex) =>
                {
                    if (element is not Label label || rowIndex < 0 || rowIndex >= _visibleRows.Count)
                        return;

                    string value = SafeGet(_visibleRows[rowIndex].Cells, columnIndex);
                    label.text = value;
                    label.tooltip = value;
                }
            };
        }

        private void PingInitializer()
        {
            if (_initializer != null)
                EditorGUIUtility.PingObject(_initializer);
        }

        private void OpenTargetFolder()
        {
            if (_initializer == null)
                return;

            EditorUtility.RevealInFinder(_initializer.TargetDirPath);
        }

        private void OpenRawFolder()
        {
            if (_initializer == null)
                return;

            EditorUtility.RevealInFinder(_initializer.DataDirPath);
        }

        private void OpenSelectedFile()
        {
            SheetPreview selectedSheet = GetSelectedSheet();
            if (selectedSheet == null)
                return;

            EditorUtility.RevealInFinder(selectedSheet.FilePath);
        }

        private void RefreshSelectedSheetFromGoogle()
        {
            SheetPreview selectedSheet = GetSelectedSheet();
            if (selectedSheet == null)
            {
                EditorUtility.DisplayDialog(L("dialog.title"), L("dialog.selectSheetFirst"), L("dialog.ok"));
                return;
            }

            if (string.IsNullOrWhiteSpace(_initializer.googleSheetUrl))
            {
                EditorUtility.DisplayDialog(L("dialog.title"), L("dialog.googleSheetMissing"), L("dialog.ok"));
                return;
            }

            bool didRefresh = _initializer.DownloadGoogleSheet(selectedSheet.Name);
            if (!didRefresh)
            {
                EditorUtility.DisplayDialog(
                    L("dialog.title"),
                    LF("dialog.sheetNotFound", selectedSheet.Name),
                    L("dialog.ok"));
                return;
            }

            DBInitializerDownloadStateStore.RecordSheetDownload(_initializer, selectedSheet.Name, DateTime.UtcNow);
            _downloadState = DBInitializerDownloadStateStore.Load(_initializer);
            ReloadPreviewData();
        }

        private void DownloadGoogleSheet()
        {
            if (string.IsNullOrWhiteSpace(_initializer.googleSheetUrl))
            {
                EditorUtility.DisplayDialog(L("dialog.title"), L("dialog.googleSheetMissing"), L("dialog.ok"));
                return;
            }

            bool didDownload = _initializer.DownloadGoogleSheet((string)null);
            if (!didDownload)
            {
                return;
            }

            DBInitializerDownloadStateStore.RecordFullDownload(_initializer, DateTime.UtcNow);
            _downloadState = DBInitializerDownloadStateStore.Load(_initializer);
            ReloadPreviewData();
        }

        private static void CopyGoogleAppsScript()
        {
            EditorGUIUtility.systemCopyBuffer = DBInitializerEditor.GoogleAppsScriptCode;
            EditorUtility.DisplayDialog(
                DBInitializerWindowLocalization.Get(ReadLanguagePref(), "dialog.title"),
                DBInitializerWindowLocalization.Get(ReadLanguagePref(), "dialog.appsScriptCopied"),
                DBInitializerWindowLocalization.Get(ReadLanguagePref(), "dialog.ok"));
        }

        private void CopySelectedRow()
        {
            if (_rowDetailField == null || string.IsNullOrWhiteSpace(_rowDetailField.value))
                return;

            EditorGUIUtility.systemCopyBuffer = _rowDetailField.value;
        }

        private void ClearRowSearch()
        {
            _searchText = string.Empty;
            _rowSearchField?.SetValueWithoutNotify(string.Empty);
            ApplyRowTransforms();
        }

        private void ResetSort()
        {
            _sortColumnIndex = -1;
            _sortDescending = false;
            SyncSortUi(null);
            ApplyRowTransforms();
        }

        private void UpdateRowHeight(int rowHeight)
        {
            _rowHeight = Mathf.Clamp(rowHeight, 24, 52);
            EditorPrefs.SetInt(RowHeightPrefKey, _rowHeight);

            if (_rowHeightSlider != null && _rowHeightSlider.value != _rowHeight)
                _rowHeightSlider.SetValueWithoutNotify(_rowHeight);

            if (_tableView != null)
            {
                _tableView.fixedItemHeight = _rowHeight;
                _tableView.Rebuild();
            }
        }

        private void ToggleSortDirection()
        {
            if (_sortColumnIndex < 0)
                return;

            _sortDescending = !_sortDescending;
            SyncSortUi(null);
            ApplyRowTransforms();
        }

        private void OnSortColumnChanged()
        {
            if (_sortColumnDropdown == null)
                return;

            _sortColumnIndex = _sortColumnDropdown.index - 1;
            if (_sortColumnIndex < 0)
                _sortDescending = false;

            SyncSortUi(null);
            ApplyRowTransforms();
        }

        private void UpdateSortControls(SheetPreview sheet)
        {
            if (_sortColumnDropdown == null || _sortDirectionButton == null)
                return;

            if (sheet == null)
            {
                _sortColumnDropdown.choices = new List<string> { L("sort.original") };
                _sortColumnDropdown.index = 0;
                _sortColumnDropdown.SetEnabled(false);
                _sortDirectionButton.SetEnabled(false);
                _sortDirectionButton.text = L("sort.asc");
                return;
            }

            var choices = new List<string> { L("sort.original") };
            choices.AddRange(sheet.Columns.Select(column => column.Name));
            _sortColumnDropdown.choices = choices;
            _sortColumnDropdown.SetEnabled(true);

            if (_sortColumnIndex >= sheet.Columns.Count)
            {
                _sortColumnIndex = -1;
                _sortDescending = false;
            }

            SyncSortUi(sheet);
        }

        private void SyncSortUi(SheetPreview sheet)
        {
            if (_sortColumnDropdown == null || _sortDirectionButton == null)
                return;

            _sortColumnDropdown.SetValueWithoutNotify(GetSortChoiceLabel(sheet));
            _sortDirectionButton.SetEnabled(_sortColumnIndex >= 0);
            _sortDirectionButton.text = _sortDescending ? L("sort.desc") : L("sort.asc");
        }

        private string GetSortChoiceLabel(SheetPreview sheet)
        {
            if (_sortColumnIndex < 0 || sheet == null || _sortColumnIndex >= sheet.Columns.Count)
                return L("sort.original");

            return sheet.Columns[_sortColumnIndex].Name;
        }

        private string BuildColumnTitle(ColumnPreview column, int columnIndex)
        {
            string title = column.Name;
            if (_sortColumnIndex == columnIndex)
                title += _sortDescending ? $" [{L("sort.descShort")}]" : $" [{L("sort.ascShort")}]";

            if (!string.IsNullOrWhiteSpace(column.Type))
                title += $" ({column.Type})";

            return title;
        }

        private void ApplyColumnHeaderTooltips(SheetPreview sheet)
        {
            if (_tableView == null)
                return;

            var headers = _tableView.Query<VisualElement>(className: "unity-multi-column-header__column").ToList();
            for (int i = 0; i < headers.Count; i++)
            {
                if (i == 0)
                {
                    headers[i].tooltip = L("tooltip.indexHeader");
                    continue;
                }

                int columnIndex = i - 1;
                if (columnIndex < 0 || columnIndex >= sheet.Columns.Count)
                    continue;

                ColumnPreview column = sheet.Columns[columnIndex];
                var parts = new List<string> { column.Name };
                if (!string.IsNullOrWhiteSpace(column.Type))
                    parts.Add(LF("tooltip.type", column.Type));
                if (!string.IsNullOrWhiteSpace(column.Comment))
                    parts.Add(LF("tooltip.comment", column.Comment));

                headers[i].tooltip = string.Join("\n", parts);
            }
        }

        private void SetSheetSummaryValues(SheetPreview sheet)
        {
            if (_sheetSourceValueLabel == null
                || _sheetRowsValueLabel == null
                || _sheetColumnsValueLabel == null
                || _sheetFileValueLabel == null
                || _previewRefreshValueLabel == null
                || _fullDownloadValueLabel == null
                || _sheetDownloadValueLabel == null)
            {
                return;
            }

            if (sheet == null)
            {
                _sheetSourceValueLabel.text = GetSourceLabel();
                _sheetSourceValueLabel.tooltip = BuildDownloadTooltip(null);
                _sheetRowsValueLabel.text = "-";
                _sheetRowsValueLabel.tooltip = L("preview.noSheet");
                _sheetColumnsValueLabel.text = "-";
                _sheetColumnsValueLabel.tooltip = L("preview.noSheet");
                _sheetFileValueLabel.text = "-";
                _sheetFileValueLabel.tooltip = BuildStateFileTooltip();
                SetTimeSummaryValues(null);
                return;
            }

            _sheetSourceValueLabel.text = GetSourceLabel();
            _sheetSourceValueLabel.tooltip = BuildDownloadTooltip(sheet.Name);
            _sheetRowsValueLabel.text = sheet.Rows.Count.ToString(CultureInfo.InvariantCulture);
            _sheetRowsValueLabel.tooltip = LF("tooltip.rowsInSheet", sheet.Rows.Count, sheet.Name);
            _sheetColumnsValueLabel.text = sheet.Columns.Count.ToString(CultureInfo.InvariantCulture);
            _sheetColumnsValueLabel.tooltip = LF("tooltip.columnsInSheet", sheet.Columns.Count, sheet.Name);
            _sheetFileValueLabel.text = Path.GetFileName(sheet.FilePath);
            _sheetFileValueLabel.tooltip = $"{sheet.FilePath}\n{BuildStateFileTooltip()}";
            SetTimeSummaryValues(sheet.Name);
        }

        private void UpdatePathSummary()
        {
            if (_pathSummaryLabel == null || _initializer == null)
                return;

            _pathSummaryLabel.text = LF("paths.summary", _initializer.DataDirPath, _initializer.TargetClassPath, _initializer.TargetDirPath);
        }

        private void UpdateSheetSummary()
        {
            int totalRows = _filteredSheets.Sum(sheet => sheet.Rows.Count);
            _sheetSummaryLabel.text = LF("sidebar.sheetCount", _filteredSheets.Count);
            _sheetCaptionLabel.text = LF("sidebar.sheetSummary", totalRows, GetSourceLabel());
        }

        private SheetPreview GetSelectedSheet()
        {
            return _filteredSheets.FirstOrDefault(sheet => sheet.Name == _selectedSheetName);
        }

        private string BuildPreviewMeta(SheetPreview sheet)
        {
            string refreshText = _lastPreviewRefreshUtc == DateTime.MinValue
                ? L("preview.notRefreshed")
                : _lastPreviewRefreshUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            if (sheet == null)
                return LF("preview.metaEmpty", GetSourceLabel(), refreshText);

            return LF("preview.meta", GetSourceLabel(), sheet.Rows.Count, sheet.Columns.Count, refreshText);
        }

        private void SetTimeSummaryValues(string sheetName)
        {
            string previewRefreshText = GetPreviewRefreshText();
            string fullDownloadText = GetFullDownloadText();
            string sheetDownloadText = GetSheetDownloadText(sheetName);

            _previewRefreshValueLabel.text = previewRefreshText;
            _previewRefreshValueLabel.tooltip = _language == UiLanguage.Korean
                ? $"마지막 미리보기 새로고침\n{previewRefreshText}"
                : $"Last preview refresh\n{previewRefreshText}";

            _fullDownloadValueLabel.text = fullDownloadText;
            _fullDownloadValueLabel.tooltip = _language == UiLanguage.Korean
                ? $"마지막 전체 Google Sheet 다운로드\n{fullDownloadText}"
                : $"Last full Google Sheet download\n{fullDownloadText}";

            _sheetDownloadValueLabel.text = sheetDownloadText;
            _sheetDownloadValueLabel.tooltip = string.IsNullOrWhiteSpace(sheetName)
                ? (_language == UiLanguage.Korean ? "시트를 선택하면 시트별 다운로드 시간이 표시됩니다." : "Select a sheet to see its per-sheet download time.")
                : (_language == UiLanguage.Korean
                    ? $"{sheetName} 시트 마지막 다운로드\n{sheetDownloadText}"
                    : $"{sheetName} last sheet download\n{sheetDownloadText}");
        }

        private string BuildDownloadTooltip(string sheetName)
        {
            string fullText = GetFullDownloadText();
            string sheetText = GetSheetDownloadText(sheetName);

            return _language == UiLanguage.Korean
                ? $"{GetSourceLabel()}\n마지막 전체 다운로드: {fullText}\n선택 시트 다운로드: {sheetText}"
                : $"{GetSourceLabel()}\nLast full download: {fullText}\nSelected sheet download: {sheetText}";
        }

        private string BuildStateFileTooltip()
        {
            string statePath = DBInitializerDownloadStateStore.TryGetStateFilePath(_initializer, out var filePath)
                ? filePath
                : (_language == UiLanguage.Korean ? "상태 파일 없음" : "No state file");

            return _language == UiLanguage.Korean
                ? $"다운로드 상태 파일\n{statePath}"
                : $"Download state file\n{statePath}";
        }

        private static bool TryFormatStoredUtc(string utcText, out string formattedText)
        {
            formattedText = null;
            if (string.IsNullOrWhiteSpace(utcText))
            {
                return false;
            }

            if (!DateTime.TryParse(
                    utcText,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                    out var utcTime))
            {
                return false;
            }

            formattedText = utcTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            return true;
        }

        private string GetPreviewRefreshText()
        {
            return _lastPreviewRefreshUtc == DateTime.MinValue
                ? (_language == UiLanguage.Korean ? "없음" : "none")
                : _lastPreviewRefreshUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        private string GetFullDownloadText()
        {
            return TryFormatStoredUtc(_downloadState?.LastFullDownloadUtc, out var formattedFull)
                ? formattedFull
                : (_language == UiLanguage.Korean ? "없음" : "none");
        }

        private string GetSheetDownloadText(string sheetName)
        {
            if (string.IsNullOrWhiteSpace(sheetName) || _downloadState?.SheetDownloadsUtc == null)
            {
                return _language == UiLanguage.Korean ? "없음" : "none";
            }

            return _downloadState.SheetDownloadsUtc.TryGetValue(sheetName, out var sheetUtc)
                   && TryFormatStoredUtc(sheetUtc, out var formattedSheet)
                ? formattedSheet
                : (_language == UiLanguage.Korean ? "없음" : "none");
        }

        private string BuildSortLabel(SheetPreview sheet)
        {
            if (_sortColumnIndex < 0 || _sortColumnIndex >= sheet.Columns.Count)
                return LF("sort.statusOriginal", _visibleRows.Count, sheet.Rows.Count);

            string direction = _sortDescending ? L("sort.descending") : L("sort.ascending");
            return LF("sort.status", sheet.Columns[_sortColumnIndex].Name, direction, _visibleRows.Count, sheet.Rows.Count);
        }

        private string GetSourceLabel()
        {
            return _previewSource == PreviewSource.DataFrame ? L("source.dataframe") : L("source.json");
        }

        private string FormatRowDetail(SheetPreview sheet, RowPreview row)
        {
            var builder = new StringBuilder();
            builder.AppendLine("{");
            builder.Append("  ");
            builder.Append(L("detail.row"));
            builder.Append(": ");
            builder.Append(row.Index + 1);
            builder.AppendLine(",");

            for (int i = 0; i < sheet.Columns.Count; i++)
            {
                builder.Append("  ");
                builder.Append(sheet.Columns[i].Name);
                builder.Append(": ");
                builder.Append(SafeGet(row.Cells, i));
                if (i < sheet.Columns.Count - 1)
                    builder.Append(',');
                builder.AppendLine();
            }

            builder.Append('}');
            return builder.ToString();
        }

        private static IEnumerable<JObject> EnumerateJsonObjects(JToken root)
        {
            if (root is JArray array)
            {
                foreach (JObject item in array.OfType<JObject>())
                    yield return item;
                yield break;
            }

            if (root is JObject obj)
                yield return obj;
        }

        private static List<string> BuildRowCells(IReadOnlyList<string> columnNames, JObject rowObject)
        {
            var cells = new List<string>(columnNames.Count);
            for (int i = 0; i < columnNames.Count; i++)
            {
                string columnName = columnNames[i];
                string value = rowObject?.TryGetValue(columnName, out JToken token) == true
                    ? token.ToString(Formatting.None)
                    : string.Empty;
                cells.Add(value);
            }

            return cells;
        }

        private static string BuildSearchText(IEnumerable<string> cells)
        {
            return string.Join(" ", cells.Where(cell => !string.IsNullOrEmpty(cell)));
        }

        private static int CompareCellValues(string left, string right)
        {
            if (decimal.TryParse(left, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal leftNumber)
                && decimal.TryParse(right, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal rightNumber))
            {
                return leftNumber.CompareTo(rightNumber);
            }

            if (DateTime.TryParse(left, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime leftDate)
                && DateTime.TryParse(right, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime rightDate))
            {
                return leftDate.CompareTo(rightDate);
            }

            if (bool.TryParse(left, out bool leftBool) && bool.TryParse(right, out bool rightBool))
                return leftBool.CompareTo(rightBool);

            return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private static string SafeGet(IReadOnlyList<string> values, int index)
        {
            return values != null && index >= 0 && index < values.Count ? values[index] ?? string.Empty : string.Empty;
        }

        private sealed class SheetPreview
        {
            public SheetPreview(string name, string filePath, List<ColumnPreview> columns, List<RowPreview> rows)
            {
                Name = name;
                FilePath = filePath;
                Columns = columns;
                Rows = rows;
            }

            public string Name { get; }
            public string FilePath { get; }
            public List<ColumnPreview> Columns { get; }
            public List<RowPreview> Rows { get; }
        }

        private sealed class ColumnPreview
        {
            public ColumnPreview(string name, string type, string comment)
            {
                Name = string.IsNullOrWhiteSpace(name) ? "(empty)" : name;
                Type = type ?? string.Empty;
                Comment = comment ?? string.Empty;
            }

            public string Name { get; }
            public string Type { get; }
            public string Comment { get; }
        }

        private sealed class RowPreview
        {
            public RowPreview(int index, List<string> cells, string searchText)
            {
                Index = index;
                Cells = cells;
                SearchText = searchText;
            }

            public int Index { get; }
            public List<string> Cells { get; }
            public string SearchText { get; }
        }

        private enum PreviewSource
        {
            DataFrame,
            Json
        }

        private enum UiLanguage
        {
            Korean = 0,
            English = 1
        }

        private static class DBInitializerWindowLocalization
        {
            private static readonly Dictionary<string, string> Korean = new Dictionary<string, string>
            {
                ["window.title"] = "DB 초기화 도구",
                ["header.kicker"] = "DATABASE TOOLKIT",
                ["header.subtitle"] = "클래스 생성, 시트 동기화, 내보낸 데이터 검사까지 한 창에서 처리합니다.",
                ["language.korean"] = "한국어",
                ["language.english"] = "English",
                ["tooltip.language"] = "에디터 창 언어를 전환합니다.",
                ["field.initializer"] = "이니셜라이저",
                ["field.source"] = "소스",
                ["field.sort"] = "정렬",
                ["field.rowHeight"] = "행 높이",
                ["button.useSelection"] = "선택 사용",
                ["button.reloadPreview"] = "미리보기 새로고침",
                ["button.pingAsset"] = "에셋 표시",
                ["button.openTargetFolder"] = "출력 폴더 열기",
                ["button.openRawFolder"] = "원본 폴더 열기",
                ["button.generateXlsx"] = "Xlsx에서 생성",
                ["button.generateJson"] = "Json에서 생성",
                ["button.downloadGoogleSheet"] = "Google Sheet 다운로드",
                ["button.copyAppsScript"] = "Apps Script 복사",
                ["button.openCacheFolder"] = "캐시 폴더 열기",
                ["button.clearImageCache"] = "이미지 캐시 삭제",
                ["button.clearSearch"] = "검색 지우기",
                ["button.resetSort"] = "정렬 초기화",
                ["button.refreshSheetFromGoogle"] = "선택 시트 갱신",
                ["button.openFile"] = "파일 열기",
                ["button.copyRow"] = "행 복사",
                ["button.copyDetail"] = "상세 복사",
                ["tooltip.useSelection"] = "현재 선택된 DBInitializer 에셋을 이 창에 바인딩합니다.",
                ["tooltip.reloadPreview"] = "현재 출력 폴더에서 시트 미리보기를 다시 불러옵니다.",
                ["tooltip.pingAsset"] = "선택된 DBInitializer 에셋을 프로젝트 창에서 강조합니다.",
                ["tooltip.openTargetFolder"] = "내보낸 데이터 폴더를 파일 탐색기에서 엽니다.",
                ["tooltip.openRawFolder"] = "원본 데이터 폴더를 파일 탐색기에서 엽니다.",
                ["tooltip.generateXlsx"] = "원본 스프레드시트 파일을 읽어 내보낸 에셋을 다시 생성합니다.",
                ["tooltip.generateJson"] = "원본 json 파일을 읽어 내보낸 에셋을 다시 생성합니다.",
                ["tooltip.downloadGoogleSheet"] = "설정된 Google Sheet URL에서 데이터를 받아와 내보냅니다.",
                ["tooltip.copyAppsScript"] = "Google Sheet 내보내기에 사용하는 Apps Script 코드를 복사합니다.",
                ["tooltip.openCacheFolder"] = "데이터베이스 도구가 사용하는 캐시 폴더를 엽니다.",
                ["tooltip.clearImageCache"] = "데이터베이스 도구가 생성한 이미지 캐시를 삭제합니다.",
                ["tooltip.source"] = "미리보기에 사용할 출력 형식을 선택합니다.",
                ["tooltip.sheetSearch"] = "시트 이름으로 시트 목록을 필터링합니다.",
                ["tooltip.sortColumn"] = "행 정렬 기준이 될 컬럼을 선택합니다.",
                ["tooltip.sortDirection"] = "오름차순/내림차순 정렬 방향을 전환합니다.",
                ["tooltip.rowHeight"] = "데이터 테이블의 행 높이를 조절합니다.",
                ["tooltip.rowSearch"] = "셀 텍스트를 기준으로 보이는 행을 필터링합니다.",
                ["tooltip.clearSearch"] = "현재 행 필터 텍스트를 지웁니다.",
                ["tooltip.resetSort"] = "선택된 시트의 원래 행 순서로 되돌립니다.",
                ["tooltip.refreshSheetFromGoogle"] = "현재 선택된 시트만 Google Sheet에서 다시 받아 갱신합니다.",
                ["tooltip.openFile"] = "현재 선택된 시트 파일을 파일 탐색기에서 엽니다.",
                ["tooltip.copyRow"] = "선택된 행의 상세 텍스트를 클립보드에 복사합니다.",
                ["tooltip.copyDetail"] = "선택된 행의 상세 텍스트를 클립보드에 복사합니다.",
                ["tooltip.index"] = "원본 파일의 {0}번째 행",
                ["tooltip.indexHeader"] = "내보낸 파일의 원본 행 번호입니다.",
                ["tooltip.type"] = "타입: {0}",
                ["tooltip.comment"] = "주석: {0}",
                ["tooltip.rowsInSheet"] = "{1} 시트의 행 수: {0}",
                ["tooltip.columnsInSheet"] = "{1} 시트의 열 수: {0}",
                ["section.paths.title"] = "해석된 경로",
                ["section.paths.desc"] = "원본 데이터, 생성 코드, 출력 파일의 실제 경로를 보여줍니다.",
                ["section.settings.title"] = "설정",
                ["section.settings.desc"] = "선택된 initializer 에셋에 직접 바인딩된 편집 가능한 필드입니다.",
                ["section.actions.title"] = "작업",
                ["section.actions.desc"] = "기본 인스펙터로 돌아가지 않고 생성과 동기화 명령을 실행합니다.",
                ["section.preview.title"] = "데이터 미리보기",
                ["section.preview.desc"] = "생성된 시트를 탐색하고 테이블로 확인하면서 메타데이터를 검증합니다.",
                ["path.raw"] = "원본 데이터",
                ["path.generated"] = "생성된 클래스",
                ["path.output"] = "출력 데이터",
                ["paths.summary"] = "원본: {0} | 생성: {1} | 출력: {2}",
                ["preview.selectSource"] = "출력 내용을 확인할 소스와 시트를 선택하세요.",
                ["preview.noSheet"] = "선택된 시트 없음",
                ["preview.notRefreshed"] = "새로고침 안 됨",
                ["preview.metaEmpty"] = "{0} | 시트 선택 대기 중 | 새로고침 {1}",
                ["preview.meta"] = "{0} | {1}행 | {2}열 | 새로고침 {3}",
                ["preview.targetDirMissing"] = "출력 폴더가 없습니다: {0}",
                ["preview.loadFailed"] = "미리보기 로드 실패: {0}",
                ["sidebar.sheets"] = "시트",
                ["sidebar.sheetsDesc"] = "현재 출력 폴더에 있는 모든 시트를 탐색합니다.",
                ["sidebar.sheetCount"] = "{0}개 시트",
                ["sidebar.sheetSummary"] = "보이는 행 {0}개 | 소스 {1}",
                ["sheet.meta"] = "{0}행 | {1}열",
                ["summary.source"] = "소스",
                ["summary.rows"] = "행",
                ["summary.columns"] = "열",
                ["summary.file"] = "파일",
                ["message.selectInitializer"] = "DBInitializer 에셋을 선택하면 경로 편집, 생성 실행, 출력 파일 검사를 할 수 있습니다.",
                ["message.selectSheet"] = "왼쪽 패널에서 시트를 선택하면 행을 확인할 수 있습니다.",
                ["detail.selectedRow"] = "선택된 행",
                ["detail.row"] = "행",
                ["sort.original"] = "원래 순서",
                ["sort.asc"] = "오름",
                ["sort.desc"] = "내림",
                ["sort.ascShort"] = "오름",
                ["sort.descShort"] = "내림",
                ["sort.ascending"] = "오름차순",
                ["sort.descending"] = "내림차순",
                ["sort.statusOriginal"] = "원래 순서 | {0}/{1}행 표시 중",
                ["sort.status"] = "{0} 기준 정렬 ({1}) | {2}/{3}행 표시 중",
                ["source.dataframe"] = "DataFrame (.df)",
                ["source.json"] = "Json Rows (.json)",
                ["table.index"] = "#",
                ["dialog.title"] = "데이터베이스",
                ["dialog.ok"] = "확인",
                ["dialog.googleSheetMissing"] = "Google Sheet URL이 설정되지 않았습니다.",
                ["dialog.appsScriptCopied"] = "Google Apps Script 코드가 클립보드에 복사되었습니다.",
                ["dialog.selectSheetFirst"] = "먼저 갱신할 시트를 선택하세요.",
                ["dialog.sheetNotFound"] = "'{0}' 시트를 Google Sheet 응답에서 찾지 못했습니다."
            };

            private static readonly Dictionary<string, string> English = new Dictionary<string, string>
            {
                ["window.title"] = "DB Initializer",
                ["header.kicker"] = "DATABASE TOOLKIT",
                ["header.subtitle"] = "Generate classes, sync sheets, inspect exported data, and validate output files in one workflow.",
                ["language.korean"] = "Korean",
                ["language.english"] = "English",
                ["tooltip.language"] = "Switch the editor window language.",
                ["field.initializer"] = "Initializer",
                ["field.source"] = "Source",
                ["field.sort"] = "Sort",
                ["field.rowHeight"] = "Row Height",
                ["button.useSelection"] = "Use Selection",
                ["button.reloadPreview"] = "Reload Preview",
                ["button.pingAsset"] = "Ping Asset",
                ["button.openTargetFolder"] = "Open Target Folder",
                ["button.openRawFolder"] = "Open Raw Folder",
                ["button.generateXlsx"] = "Generate From Xlsx",
                ["button.generateJson"] = "Generate From Json",
                ["button.downloadGoogleSheet"] = "Download Google Sheet",
                ["button.copyAppsScript"] = "Copy Apps Script",
                ["button.openCacheFolder"] = "Open Cache Folder",
                ["button.clearImageCache"] = "Clear Image Cache",
                ["button.clearSearch"] = "Clear Search",
                ["button.resetSort"] = "Reset Sort",
                ["button.refreshSheetFromGoogle"] = "Refresh Sheet",
                ["button.openFile"] = "Open File",
                ["button.copyRow"] = "Copy Row",
                ["button.copyDetail"] = "Copy Detail",
                ["tooltip.useSelection"] = "Bind the currently selected DBInitializer asset to this window.",
                ["tooltip.reloadPreview"] = "Reload sheet previews from the current output directory.",
                ["tooltip.pingAsset"] = "Ping the selected DBInitializer asset in the Project window.",
                ["tooltip.openTargetFolder"] = "Open the exported data folder in the file explorer.",
                ["tooltip.openRawFolder"] = "Open the raw source data folder in the file explorer.",
                ["tooltip.generateXlsx"] = "Read raw spreadsheet files and regenerate exported assets.",
                ["tooltip.generateJson"] = "Read raw json files and regenerate exported assets.",
                ["tooltip.downloadGoogleSheet"] = "Fetch sheet data from the configured Google Sheet URL and export it.",
                ["tooltip.copyAppsScript"] = "Copy the helper Apps Script snippet used for Google Sheet export.",
                ["tooltip.openCacheFolder"] = "Open the persistent cache folder used by the database tools.",
                ["tooltip.clearImageCache"] = "Delete cached images generated by the database tooling.",
                ["tooltip.source"] = "Choose which export format should be previewed.",
                ["tooltip.sheetSearch"] = "Filter the sheet list by sheet name.",
                ["tooltip.sortColumn"] = "Choose which column should drive row sorting.",
                ["tooltip.sortDirection"] = "Toggle between ascending and descending sort order.",
                ["tooltip.rowHeight"] = "Adjust the visible row height for the data table.",
                ["tooltip.rowSearch"] = "Filter visible rows by matching any cell text.",
                ["tooltip.clearSearch"] = "Clear the current row filter text.",
                ["tooltip.resetSort"] = "Restore the original row order for the selected sheet.",
                ["tooltip.refreshSheetFromGoogle"] = "Refresh only the currently selected sheet from Google Sheet.",
                ["tooltip.openFile"] = "Reveal the currently selected sheet file in the file explorer.",
                ["tooltip.copyRow"] = "Copy the selected row detail text to the clipboard.",
                ["tooltip.copyDetail"] = "Copy the selected row detail text to the clipboard.",
                ["tooltip.index"] = "Original exported row {0}",
                ["tooltip.indexHeader"] = "Original row index from the exported file.",
                ["tooltip.type"] = "Type: {0}",
                ["tooltip.comment"] = "Comment: {0}",
                ["tooltip.rowsInSheet"] = "{0} rows in {1}",
                ["tooltip.columnsInSheet"] = "{0} columns in {1}",
                ["section.paths.title"] = "Resolved Paths",
                ["section.paths.desc"] = "The editor keeps live references to source, generated code, and exported output directories.",
                ["section.settings.title"] = "Settings",
                ["section.settings.desc"] = "Editable fields are bound directly to the selected initializer asset.",
                ["section.actions.title"] = "Actions",
                ["section.actions.desc"] = "Use the same generation and sync commands without dropping back to the default inspector.",
                ["section.preview.title"] = "Data Preview",
                ["section.preview.desc"] = "Browse generated sheets, inspect rows as a table, and validate metadata before runtime hits the files.",
                ["path.raw"] = "Raw Data",
                ["path.generated"] = "Generated Classes",
                ["path.output"] = "Export Output",
                ["paths.summary"] = "Raw: {0} | Generated: {1} | Output: {2}",
                ["preview.selectSource"] = "Select a source and sheet to inspect exported content.",
                ["preview.noSheet"] = "No sheet selected",
                ["preview.notRefreshed"] = "not refreshed",
                ["preview.metaEmpty"] = "{0} | waiting for sheet selection | refreshed {1}",
                ["preview.meta"] = "{0} | {1} rows | {2} columns | refreshed {3}",
                ["preview.targetDirMissing"] = "Target directory does not exist: {0}",
                ["preview.loadFailed"] = "Failed to load preview data: {0}",
                ["sidebar.sheets"] = "Sheets",
                ["sidebar.sheetsDesc"] = "Browse every exported sheet from the selected target directory.",
                ["sidebar.sheetCount"] = "{0} sheets",
                ["sidebar.sheetSummary"] = "{0} rows in view | source {1}",
                ["sheet.meta"] = "{0} rows | {1} columns",
                ["summary.source"] = "Source",
                ["summary.rows"] = "Rows",
                ["summary.columns"] = "Columns",
                ["summary.file"] = "File",
                ["message.selectInitializer"] = "Select a DBInitializer asset to edit paths, run generation, and inspect exported files.",
                ["message.selectSheet"] = "Select a sheet from the left panel to inspect rows.",
                ["detail.selectedRow"] = "Selected Row",
                ["detail.row"] = "row",
                ["sort.original"] = "Original Order",
                ["sort.asc"] = "Asc",
                ["sort.desc"] = "Desc",
                ["sort.ascShort"] = "asc",
                ["sort.descShort"] = "desc",
                ["sort.ascending"] = "ascending",
                ["sort.descending"] = "descending",
                ["sort.statusOriginal"] = "Original order | showing {0}/{1} rows",
                ["sort.status"] = "Sorted by {0} ({1}) | showing {2}/{3} rows",
                ["source.dataframe"] = "DataFrame (.df)",
                ["source.json"] = "Json Rows (.json)",
                ["table.index"] = "#",
                ["dialog.title"] = "Database",
                ["dialog.ok"] = "OK",
                ["dialog.googleSheetMissing"] = "Google Sheet URL is not set.",
                ["dialog.appsScriptCopied"] = "Google Apps Script code copied to clipboard.",
                ["dialog.selectSheetFirst"] = "Select a sheet to refresh first.",
                ["dialog.sheetNotFound"] = "Sheet '{0}' was not found in the Google Sheet response."
            };

            public static string Get(UiLanguage language, string key)
            {
                var table = language == UiLanguage.Korean ? Korean : English;
                return table.TryGetValue(key, out string value) ? value : key;
            }
        }
    }
}
