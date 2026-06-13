import bpy
import json

def write_data(filepath, fileData):

    # Saving the gltf file with level geometry
    bpy.ops.export_scene.fbx(filepath=filepath, collection="Level Geometry")

    # Saving the jSON file with level object data
    f = open(filepath.replace(".fbx", ".json"), "w", encoding='utf-8')
    f.write(json.dumps(fileData))
    f.close()

    return {'FINISHED'}

from bpy_extras.io_utils import ExportHelper
from bpy.props import StringProperty
from bpy.types import Operator


class ExportSomeData(Operator, ExportHelper):
    """Exports the level model and level objects' custom properties"""
    bl_idname = "sonic_collision.level_data"
    bl_label = "Export Level Data"

    filename_ext = ".fbx"

    filter_glob: StringProperty = StringProperty(
        default="*.glb",
        options={'HIDDEN'},
        maxlen=255,
    )

    def execute(self, context):
        # Level Objects collection, duh
        collection = bpy.data.collections.get("Level Objects")

        fileOutput = []

        # Processing data for the objects JSON
        for obj in collection.objects:

            # Initialising the data dump variable and common info
            fileData = {"parameters":{}}
            fileData["name"] = obj.name
            fileData["position"] = [obj.location.x, obj.location.y, obj.location.z]
            fileData["rotation"] = [obj.rotation_euler.x, obj.rotation_euler.y, obj.rotation_euler.z]

            for key in obj.keys():
                if key == "type":
                    fileData[key] = obj[key]
                    continue
                fileData["parameters"][key] = obj[key]

            # Checking if there's data to export
            if obj.data is None:
                fileOutput.append(fileData)
                continue

            # Exporting the curve if there is one
            if obj.data.id_type == "CURVE":
                print("Curve!")
                fileData["curve"] = []
                for point in obj.data.splines[0].bezier_points:
                    fileData["curve"].append([point.co.x,point.co.y,point.co.z,
                                              point.handle_left.x,point.handle_left.y,point.handle_left.z,
                                              point.handle_right.x,point.handle_right.y,point.handle_right.z])

            fileOutput.append(fileData)
        
        # Write data
        return write_data(self.filepath, fileOutput)


def menu_func_export(self, context):
    self.layout.operator(ExportSomeData.bl_idname, text="Export level data")


# Register and add to the "file selector" menu (required to use F3 search "Text Export Operator" for quick access).
def register():
    bpy.utils.register_class(ExportSomeData)
    bpy.types.TOPBAR_MT_file_export.append(menu_func_export)


def unregister():
    bpy.utils.unregister_class(ExportSomeData)
    bpy.types.TOPBAR_MT_file_export.remove(menu_func_export)


if __name__ == "__main__":
    register()

    # Test call.
    bpy.ops.sonic_collision.level_data('INVOKE_DEFAULT')
