using System;
using ImageEditorWeb.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageEditorWeb.Core.Services
{
    public class LayerService
    {
        private List<CanvasLayer> _layers = new();
        private readonly Stack<List<CanvasLayer>> _history = new();
        private readonly Stack<List<CanvasLayer>> _redoStack = new();
        
        public IReadOnlyList<CanvasLayer> Layers => _layers.AsReadOnly();
        public void AddLayer(CanvasLayer layer)
        {
            SaveState();
            _layers.Add(layer);
            SortLayers();
        }
        public void RemoveLayer(Guid layerId)
        {
            SaveState();
            _layers.RemoveAll(I => I.Id == layerId);
        }

        public void MoveLayerUp(Guid layerId)
        {
            SaveState();
            var layer = _layers.FirstOrDefault(I => I.Id == layerId);
            if (layer != null)
            {
                layer.ZIndex++;
                SortLayers();
            }
        }

        private void SortLayers()
        {
            _layers = _layers.OrderBy(I => I.ZIndex).ToList();
        }
        private void SaveState()
        {
            var stateCopy = _layers.Select(I => new CanvasLayer
            {
                Id = I.Id,
                Name = I.Name,
                ImageData = I.ImageData?.ToArray(),
                ZIndex = I.ZIndex,
                Opacity = I.Opacity,
                IsVisible = I.IsVisible
            }).ToList();

            _history.Push(stateCopy);
            _redoStack.Clear();
        }

        public bool CanUndo() => _history.Count > 0;
        public bool CanRedo() => _redoStack.Count > 0;

        public void Undo()
        {
            if (_history.Count > 0)
            {
                _redoStack.Push(_layers.ToList());
                _layers = _history.Pop();
            }
        }
        public void Redo() 
        { 
            if(_redoStack.Count > 0) 
            {
                _history.Push(_layers.ToList());
                _layers = _redoStack.Pop();
            }
        }
    }
}
