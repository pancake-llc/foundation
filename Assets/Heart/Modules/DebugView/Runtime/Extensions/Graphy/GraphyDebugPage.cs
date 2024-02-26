#if PANCAKE_GRAPHY
using System;
using System.Collections;
using Tayx.Graphy;
using System.Threading.Tasks;

namespace Pancake.DebugView
{
    public sealed class GraphyDebugPage : DefaultDebugPageBase
    {
        private GraphyManager _graphyManager;
        private EnumPickerCellModel _fpsStatePickerModel;
        private EnumPickerCellModel _ramStatePickerModel;
        private EnumPickerCellModel _audioStatePickerModel;
        private EnumPickerCellModel _advancedStatePickerModel;
        protected override string Title => "Debug Tools";

        public void Setup(GraphyManager graphyManager) { _graphyManager = graphyManager; }

        public override Task Initialize()
        {
            // FPS
            var fpsStatePickerModel = new EnumPickerCellModel(_graphyManager.FpsModuleState);
            _fpsStatePickerModel = fpsStatePickerModel;
            fpsStatePickerModel.Text = "FPS";
            fpsStatePickerModel.ActiveValueChanged += OnFPSStatePickerValueChanged;
            AddEnumPicker(fpsStatePickerModel);

            // RAM

            var ramStatePickerModel = new EnumPickerCellModel(_graphyManager.RamModuleState);
            _ramStatePickerModel = ramStatePickerModel;
            ramStatePickerModel.Text = "RAM";
            ramStatePickerModel.ActiveValueChanged += OnRAMStatePickerValueChanged;
            AddEnumPicker(ramStatePickerModel);

            // Audio
            var audioStatePickerModel = new EnumPickerCellModel(_graphyManager.AudioModuleState);
            _audioStatePickerModel = audioStatePickerModel;
            audioStatePickerModel.Text = "Audio";
            audioStatePickerModel.ActiveValueChanged += OnAudioStatePickerValueChanged;
            AddEnumPicker(audioStatePickerModel);

            // Advanced
            var advancedStatePickerModel = new EnumPickerCellModel(_graphyManager.AdvancedModuleState);
            _advancedStatePickerModel = advancedStatePickerModel;
            advancedStatePickerModel.Text = "Advanced";
            advancedStatePickerModel.ActiveValueChanged += OnAdvancedStatePickerValueChanged;
            AddEnumPicker(advancedStatePickerModel);

            Reload();

            return Task.CompletedTask;
        }

        public override Task Cleanup()
        {
            _fpsStatePickerModel.ActiveValueChanged -= OnFPSStatePickerValueChanged;
            _ramStatePickerModel.ActiveValueChanged -= OnRAMStatePickerValueChanged;
            _audioStatePickerModel.ActiveValueChanged -= OnAudioStatePickerValueChanged;
            _advancedStatePickerModel.ActiveValueChanged -= OnAdvancedStatePickerValueChanged;

            return Task.CompletedTask;
        }

        private void OnFPSStatePickerValueChanged(Enum value)
        {
            var state = (GraphyManager.ModuleState) value;
            _graphyManager.FpsModuleState = state;
        }

        private void OnRAMStatePickerValueChanged(Enum value)
        {
            var state = (GraphyManager.ModuleState) value;
            _graphyManager.RamModuleState = state;
        }

        private void OnAudioStatePickerValueChanged(Enum value)
        {
            var state = (GraphyManager.ModuleState) value;
            _graphyManager.AudioModuleState = state;
        }

        private void OnAdvancedStatePickerValueChanged(Enum value)
        {
            var state = (GraphyManager.ModuleState) value;
            _graphyManager.AdvancedModuleState = state;
        }
    }
}
#endif