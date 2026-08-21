from ScriptCollection.TFCPS.DotNet.TFCPS_CodeUnitSpecific_DotNet import TFCPS_CodeUnitSpecific_DotNet_Functions,TFCPS_CodeUnitSpecific_DotNet_CLI
 
def build():

    tf:TFCPS_CodeUnitSpecific_DotNet_Functions=TFCPS_CodeUnitSpecific_DotNet_CLI.parse(__file__)
    # the runtimes this codeunit is built for are stated by its project-file (RuntimeIdentifiers)
    tf.build(False)


if __name__ == "__main__":
    build()
