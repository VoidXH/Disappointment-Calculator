using System.Text.Json;

namespace DisappointmentCalculator.Utilities;

public static class JsonExtensions {
    /// <summary>
    /// Safely navigates a nested JSON structure using a dot-separated path.
    /// </summary>
    /// <param name="root">The root JsonElement</param>
    /// <param name="element">The found JsonElement, or default if not found</param>
    /// <param name="path">Each variable name under which the next object and finally the value is found</param>
    /// <returns>True if the property path was successfully resolved; otherwise, false.</returns>
    public static bool TryGetNestedProperty(this JsonElement root, out JsonElement element, params string[] path) {
        element = root;
        foreach (string location in path) {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(location, out element)) {
                element = default;
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Checks if a property exists, is a non-empty array, and its last element matches the specified string.
    /// </summary>
    /// <param name="root">The root JsonElement</param>
    /// <param name="propertyName">The name of the property to check</param>
    /// <param name="expectedLastValue">The expected string value of the last element</param>
    /// <returns>True if the condition is met; otherwise, false.</returns>
    public static bool LastArrayElementEquals(this JsonElement root, string propertyName, string expectedLastValue) {
        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(propertyName, out JsonElement arrayProp)) {
            if (arrayProp.ValueKind == JsonValueKind.Array) {
                int length = arrayProp.GetArrayLength();
                if (length > 0) {
                    JsonElement lastElement = arrayProp[length - 1];
                    return lastElement.ValueKind == JsonValueKind.String && lastElement.GetString() == expectedLastValue;
                }
            }
        }
        return false;
    }
}
