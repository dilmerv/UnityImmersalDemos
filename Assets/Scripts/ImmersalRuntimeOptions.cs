using Immersal.AR;
using TMPro;
using UnityEngine;

public class ImmersalRuntimeOptions : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI pointCloudVisualizerText;
    
    private bool areVisualizersActive = true;
    
    public void ToggleVisualizations()
    {
        areVisualizersActive = !areVisualizersActive;
        var visualizers = FindObjectsOfType<ARMapVisualization>();
        foreach (var visualizer in visualizers)
        {
            visualizer.renderMode = !areVisualizersActive ? ARMapVisualization.RenderMode.DoNotRender : 
                ARMapVisualization.RenderMode.EditorAndRuntime;

            pointCloudVisualizerText.text = !areVisualizersActive ? "Visualizers Off" : "Visualizers On";
        }
    }
}
