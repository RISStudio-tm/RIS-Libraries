﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;

namespace RIS.Collections.Trees
{
    internal class TrieNode<T>
    {
        public TrieNode<T> this[char c]
        {
            get
            {
                if (_nodes != null
                    && KeyPart <= c
                    && c < KeyPart + _nodes.Length)
                {
                    return _nodes[c - KeyPart];
                }

                return null;
            }
        }



        private TrieNode<T>[] _nodes;
        public IEnumerable<TrieNode<T>> Nodes
        {
            get
            {
                return _nodes;
            }
        }
        public char KeyPart { get; private set; }
        public T Value { get; set; }
        public bool IsEnd { get; set; }



        public TrieNode()
        {
            _nodes = null;

            KeyPart = '\0';
            Value = default;
            IsEnd = false;
        }



        public TrieNode<T> AddChild(char keyPart)
        {
            if (_nodes == null)
            {
                KeyPart = keyPart;
                _nodes = new TrieNode<T>[1];
            }
            else if (keyPart >= KeyPart + _nodes.Length)
            {
                Array.Resize(ref _nodes, keyPart - KeyPart + 1);
            }
            else if (keyPart < KeyPart)
            {
                var newKeyPart = (char)(KeyPart - keyPart);
                var tempNodes = new TrieNode<T>[_nodes.Length + newKeyPart];

                _nodes.CopyTo(tempNodes, newKeyPart);

                KeyPart = keyPart;
                _nodes = tempNodes;
            }

            var node = _nodes[keyPart - KeyPart];

            if (node != null)
                return node;

            node = new TrieNode<T>();
            _nodes[keyPart - KeyPart] = node;

            return node;
        }
    };
}
