import {DOMImplementation} from "@xmldom/xmldom";
import type {Document, Element, Node} from "@xmldom/xmldom";

const domImplementation = new DOMImplementation();

/**
 * Fast clone of a Document without the overhead of xmldom.
 * @param source
 */
export function fastClone(source: Document): Document {
    const copy = domImplementation.createDocument(null, "", null);
    copyChildren(copy, source, copy);
    return copy;
}

function copyChildren(doc: Document, from: Node, to: Node): void {
    for (let node = from.firstChild; node; node = node.nextSibling) {
        const copy = copyNode(doc, node);
        if (copy) to.appendChild(copy);
    }
}

function copyNode(doc: Document, node: Node): Node | null {
    switch (node.nodeType) {
        case node.ELEMENT_NODE: {
            const source = node as Element;
            const element = doc.createElementNS(source.namespaceURI, source.tagName);
            for (let i = 0; i < source.attributes.length; i++) {
                const attr = source.attributes[i];
                element.setAttributeNS(attr.namespaceURI, attr.name, attr.value);
            }
            copyChildren(doc, source, element);
            return element;
        }
        case node.TEXT_NODE:
            return doc.createTextNode(node.nodeValue ?? "");
        case node.CDATA_SECTION_NODE:
            return doc.createCDATASection(node.nodeValue ?? "");
        case node.COMMENT_NODE:
            return doc.createComment(node.nodeValue ?? "");
        case node.PROCESSING_INSTRUCTION_NODE:
            return doc.createProcessingInstruction(node.nodeName, node.nodeValue ?? "");
        default:
            return null;
    }
}
