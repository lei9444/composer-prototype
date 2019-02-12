// this is currently hard-coded here

import JsonEditor from 'JsonEditor'

const EditorMap = {
    '.bot' : JsonEditor,
    '.dialog':JsonEditor,
    '.json': JsonEditor,
    '.lg': JsonEditor,
    '.lu': JsonEditor,
    '.md': JsonEditor
}

const getEditor = (documentType) => {
    if(EditorMap[documentType]) {
        return EditorMap[documentType]
    } else {
        return null
    }
}

export default getEditor