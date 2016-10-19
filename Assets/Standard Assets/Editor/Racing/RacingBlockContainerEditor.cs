using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RacingBlockContainer))]
[CanEditMultipleObjects]
public class RacingBlockContainerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        bool resetTokenVerticalOrientation = EditorGUILayout.Toggle("ResetTokenVerticalOrientation", ((RacingBlockContainer)target).resetTokenVerticalOrientation);
        foreach (RacingBlockContainer r in targets)
            ((RacingBlockContainer)r).resetTokenVerticalOrientation = resetTokenVerticalOrientation;

        RacingBlockContainer container;
        Transform transform;

        RacingBlockContainerUtils utils = EditorWindow.GetWindow<RacingBlockContainerUtils>("Racing Blocks", false);

        if (serializedObject.isEditingMultipleObjects)
        {
            if (GUILayout.Button("Update All"))
            {
                List<RacingBlockContainer> tails = new List<RacingBlockContainer>();
                foreach (UnityEngine.Object obj in targets)
                {
                    container = obj as RacingBlockContainer;

                    if (null == container.next)
                        tails.Add(container);
                }

                foreach (RacingBlockContainer tail in tails)
                {
                    container = tail;
                    while (container != null)
                    {
                        transform = container.gameObject.transform;

                        RacingBlock oldBlock = container.block;
                        if (oldBlock != null)
                        {
                            if (container.prev != null && container.prev.block != null)
                                container.prev.block.lastToken.nextLinks.Remove(oldBlock.firstToken);

                            if (container.next != null && container.next.block != null)
                                container.next.block.firstToken.prevLinks.Remove(oldBlock.lastToken);

                            GameObject.DestroyImmediate(oldBlock.gameObject);
                            oldBlock = null;
                        }

                        RacingBlock newBlock = GameObject.Instantiate(container.selectedBlockPrefab/*utils.blocks[container.selectedBlock]*/, transform.position, transform.rotation) as RacingBlock;
                        newBlock.gameObject.transform.parent = transform;
                        container.block = newBlock;

                        if (container.prev != null && container.prev.block != null)
                        {
                            newBlock.firstToken.prevLinks.Add(container.prev.block.lastToken);
                            container.prev.block.lastToken.nextLinks.Add(newBlock.firstToken);
                        }
                        if (container.next != null && container.next.block != null)
                        {
                            newBlock.lastToken.nextLinks.Add(container.next.block.firstToken);
                            container.next.block.firstToken.prevLinks.Add(newBlock.lastToken);
                        }

                        container.MoveNextBlocks();

                        container = container.prev;
                    }
                }
            }

            return;
        }

        container = target as RacingBlockContainer;
        transform = container.gameObject.transform;

        RacingBlockContainer newPrev = EditorGUILayout.ObjectField("Prev", container.prev, typeof(RacingBlockContainer), true) as RacingBlockContainer;
        RacingBlockContainer newNext = EditorGUILayout.ObjectField("Next", container.next, typeof(RacingBlockContainer), true) as RacingBlockContainer;

        if (GUI.changed)
        {
            if (newPrev != container.prev)
                container.SetPrevious(newPrev);

            if (newNext != container.next)
                container.SetNext(newNext);
        }

        //container.selectedBlock = EditorGUILayout.Popup("Block", container.selectedBlock, utils.blockNames);
        int oldSelBlock = 0;
        foreach (RacingBlock block in utils.blocks)
        {
            if (block == container.selectedBlockPrefab)
                break;
            ++oldSelBlock;
        }
        int newSelBlock = EditorGUILayout.Popup("Block", oldSelBlock/*container.selectedBlock*/, utils.blockNames);
        if (newSelBlock != oldSelBlock)
        {
            container.selectedBlockPrefab = utils.blocks[newSelBlock];
        }

        if (GUILayout.Button("Update"))
        {
            RacingBlock oldBlock = container.block;
            if (oldBlock != null)
            {
                if (container.prev != null && container.prev.block != null)
                    container.prev.block.lastToken.nextLinks.Remove(oldBlock.firstToken);

                if (container.next != null && container.next.block != null)
                    container.next.block.firstToken.prevLinks.Remove(oldBlock.lastToken);

                GameObject.DestroyImmediate(oldBlock.gameObject);
                oldBlock = null;
            }

            RacingBlock newBlock = GameObject.Instantiate(container.selectedBlockPrefab/*utils.blocks[container.selectedBlock]*/, transform.position, transform.rotation) as RacingBlock;
            newBlock.gameObject.transform.parent = transform;
            container.block = newBlock;

            if (container.prev != null && container.prev.block != null)
            {
                newBlock.firstToken.prevLinks.Add(container.prev.block.lastToken);
                container.prev.block.lastToken.nextLinks.Add(newBlock.firstToken);
            }
            if (container.next != null && container.next.block != null)
            {
                newBlock.lastToken.nextLinks.Add(container.next.block.firstToken);
                container.next.block.firstToken.prevLinks.Add(newBlock.lastToken);
            }

            container.MoveNextBlocks();
        }
        if (GUILayout.Button("Insert Before"))
        {
            Selection.activeGameObject = container.InsertBefore();
        }
        if (GUILayout.Button("Insert After"))
        {
            Selection.activeGameObject = container.InsertAfter();
        }
        if (GUILayout.Button("Delete"))
        {
            container.Remove();

            if (container.prev != null)
                container.prev.MoveNextBlocks();

            GameObject.DestroyImmediate(container.gameObject);
        }
    }
}
