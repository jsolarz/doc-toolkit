// Document Viewer Application
class DocumentViewer {
    constructor() {
        this.documents = [];
        this.navigation = null;
        this.currentDocument = null;
        this.marked = null;
        this.init();
    }

    async init() {
        this.initTheme();
        this.setupEventListeners();
        await this.initMarked();
        // Initialize welcome screen
        const welcomeScreen = document.getElementById('welcomeScreen');
        if (welcomeScreen) {
            welcomeScreen.innerHTML = this.getWelcomeContent();
        }
        await this.loadDocuments();
    }

    async initMarked() {
        try {
            // Import marked.js from CDN with timeout
            const importPromise = import('https://cdn.jsdelivr.net/npm/marked@15.0.12/+esm');
            const timeoutPromise = new Promise((_, reject) =>
                setTimeout(() => reject(new Error('marked.js load timeout')), 10000)
            );

            const { marked } = await Promise.race([importPromise, timeoutPromise]);

            // Configure marked.js options for better security and rendering
            if (marked.setOptions) {
                marked.setOptions({
                    breaks: false,
                    gfm: true,
                    headerIds: true,
                    mangle: false
                });
            }

            this.marked = marked;
        } catch (error) {
            console.warn('Failed to initialize marked.js, using fallback parser:', error.message);
            // Fallback to custom parser if marked.js fails
            this.marked = null;
        }
    }

    initTheme() {
        const savedTheme = localStorage.getItem('theme') || 'light';
        this.setTheme(savedTheme);
    }

    setTheme(theme) {
        const root = document.documentElement;
        const themeIcon = document.getElementById('themeIcon');
        const themeText = document.getElementById('themeText');

        if (theme === 'dark') {
            root.setAttribute('data-theme', 'dark');
            if (themeIcon) themeIcon.textContent = '☀️';
            if (themeText) themeText.textContent = 'Light';
        } else {
            root.removeAttribute('data-theme');
            if (themeIcon) themeIcon.textContent = '🌙';
            if (themeText) themeText.textContent = 'Dark';
        }

        localStorage.setItem('theme', theme);
    }

    toggleTheme() {
        const currentTheme = document.documentElement.getAttribute('data-theme');
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        this.setTheme(newTheme);
    }

    setupEventListeners() {
        const homeBtn = document.getElementById('homeBtn');
        if (homeBtn) {
            homeBtn.addEventListener('click', () => this.goHome());
        }

        const refreshBtn = document.getElementById('refreshBtn');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', () => this.loadDocuments());
        }

        const themeToggle = document.getElementById('themeToggle');
        if (themeToggle) {
            themeToggle.addEventListener('click', () => this.toggleTheme());
        }

        const searchInput = document.getElementById('searchInput');
        const searchBtn = document.getElementById('searchBtn');

        if (searchInput) {
            searchInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    this.performSearch();
                }
            });
        }

        if (searchBtn) {
            searchBtn.addEventListener('click', () => this.performSearch());
        }
    }

    async loadDocuments() {
        const listElement = document.getElementById('documentList');
        const welcomeScreen = document.getElementById('welcomeScreen');
        if (!listElement) return;

        // Show welcome screen when loading documents (if no document is selected)
        if (welcomeScreen && !this.currentDocument) {
            welcomeScreen.style.display = 'block';
        }

        listElement.innerHTML = '<div class="loading">Loading documents...</div>';

        try {
            const response = await fetch('/api/documents', {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();
            this.documents = Array.isArray(data.documents) ? data.documents : [];
            this.navigation = data.navigation || null;
            this.renderNavigation();
        } catch (error) {
            console.error('Error loading documents:', error);
            listElement.innerHTML = `
                <div class="error">
                    <p>Error loading documents: ${this.escapeHtml(error.message)}</p>
                    <button class="btn-link" onclick="documentViewer.loadDocuments()">Retry</button>
                </div>
            `;
        }
    }

    renderNavigation() {
        const listElement = document.getElementById('documentList');
        if (!listElement) return;

        if (!this.navigation || !this.navigation.children || this.navigation.children.length === 0) {
            listElement.innerHTML = `
                <div class="empty">
                    <div class="empty-icon">📄</div>
                    <p>No documents found</p>
                    <p style="font-size: 0.875rem; margin-top: 0.5rem; color: var(--text-muted);">
                        Generate documents using:<br>
                        <code style="margin-top: 0.5rem; display: inline-block;">doc generate prd "Document Name"</code>
                    </p>
                </div>
            `;
            return;
        }

        listElement.innerHTML = this.renderNavigationNode(this.navigation);
        this.attachNavigationHandlers();
    }

    renderNavigationNode(node, level = 0) {
        if (node.isFile) {
            return `
                <div class="nav-item nav-file" data-path="${this.escapeHtml(node.path)}" data-level="${level}">
                    <div class="nav-item-content">
                        <span class="nav-item-name">${this.escapeHtml(node.name)}</span>
                        ${node.type ? `<span class="document-type">${this.escapeHtml(node.type)}</span>` : ''}
                    </div>
                </div>
            `;
        }

        const children = (node.children || []).map(child => this.renderNavigationNode(child, level + 1)).join('');
        const hasChildren = node.children && node.children.length > 0;

        return `
            <div class="nav-folder" data-level="${level}">
                ${node.name ? `<div class="nav-folder-header">
                    <span class="nav-folder-name">${this.escapeHtml(node.name)}</span>
                </div>` : ''}
                ${hasChildren ? `<div class="nav-folder-children">${children}</div>` : ''}
            </div>
        `;
    }

    attachNavigationHandlers() {
        const listElement = document.getElementById('documentList');
        if (!listElement) return;

        listElement.querySelectorAll('.nav-file').forEach(item => {
            item.addEventListener('click', (e) => {
                e.stopPropagation();
                const path = item.getAttribute('data-path');
                if (path) {
                    this.loadDocument(path);

                    // Update active state
                    listElement.querySelectorAll('.nav-file, .nav-item').forEach(i => i.classList.remove('active'));
                    item.classList.add('active');
                }
            });
        });

        listElement.querySelectorAll('.nav-folder-header').forEach(header => {
            header.addEventListener('click', (e) => {
                e.stopPropagation();
                const folder = header.closest('.nav-folder');
                const children = folder.querySelector('.nav-folder-children');
                if (children) {
                    folder.classList.toggle('collapsed');
                }
            });
        });
    }

    async performSearch() {
        const searchInput = document.getElementById('searchInput');
        if (!searchInput) return;

        const query = searchInput.value.trim();
        if (!query) {
            this.loadDocuments();
            return;
        }

        // Debounce: don't search for very short queries
        if (query.length < 2) {
            return;
        }

        const listElement = document.getElementById('documentList');
        if (!listElement) return;

        listElement.innerHTML = '<div class="loading">Searching...</div>';

        try {
            const encodedQuery = encodeURIComponent(query);
            const response = await fetch(`/api/search?q=${encodedQuery}`, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();
            this.renderSearchResults(Array.isArray(data.results) ? data.results : [], query);
        } catch (error) {
            console.error('Search error:', error);
            listElement.innerHTML = `
                <div class="error">
                    <p>Search error: ${this.escapeHtml(error.message)}</p>
                    <button class="btn-link" onclick="documentViewer.loadDocuments()">Clear Search</button>
                </div>
            `;
        }
    }

    renderSearchResults(results, query) {
        const listElement = document.getElementById('documentList');
        if (!listElement) return;

        if (results.length === 0) {
            listElement.innerHTML = `
                <div class="empty">
                    <div class="empty-icon">🔍</div>
                    <p>No results found for "${this.escapeHtml(query)}"</p>
                </div>
            `;
            return;
        }

        listElement.innerHTML = `
            <div class="search-results-header">
                <h3>Search Results (${results.length})</h3>
                <button class="btn-link" onclick="documentViewer.loadDocuments()">Clear</button>
            </div>
            ${results.map(result => `
                <div class="search-result-item" data-path="${this.escapeHtml(result.path)}">
                    <div class="search-result-header">
                        <div class="search-result-name">${this.escapeHtml(result.name)}</div>
                        <span class="document-type">${this.escapeHtml(result.type)}</span>
                    </div>
                    <div class="search-result-snippet">${this.highlightSearchTerms(this.escapeHtml(result.snippet), query)}</div>
                </div>
            `).join('')}
        `;

        listElement.querySelectorAll('.search-result-item').forEach(item => {
            item.addEventListener('click', () => {
                const path = item.getAttribute('data-path');
                this.loadDocument(path);
            });
        });
    }

    highlightSearchTerms(text, query) {
        const terms = query.toLowerCase().split(' ').filter(t => t.length > 0);
        let highlighted = text;
        terms.forEach(term => {
            const regex = new RegExp(`(${this.escapeRegex(term)})`, 'gi');
            highlighted = highlighted.replace(regex, '<mark>$1</mark>');
        });
        return highlighted;
    }

    escapeRegex(str) {
        return str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }

    goHome() {
        const viewerElement = document.getElementById('documentViewer');
        const tocElement = document.getElementById('tableOfContents');
        const welcomeScreen = document.getElementById('welcomeScreen');
        
        // Clear current document
        this.currentDocument = null;
        
        // Show welcome screen
        if (welcomeScreen) {
            welcomeScreen.style.display = 'block';
            welcomeScreen.innerHTML = this.getWelcomeContent();
        }
        
        // Clear TOC
        if (tocElement) {
            tocElement.innerHTML = '';
        }
        
        // Clear viewer if welcome screen doesn't exist
        if (viewerElement && !welcomeScreen) {
            viewerElement.innerHTML = '<div class="welcome" id="welcomeScreen">' + this.getWelcomeContent() + '</div>';
        }
        
        // Remove active state from navigation items
        const listElement = document.getElementById('documentList');
        if (listElement) {
            listElement.querySelectorAll('.nav-file, .nav-item').forEach(item => {
                item.classList.remove('active');
            });
        }
    }

    async loadDocument(path) {
        const viewerElement = document.getElementById('documentViewer');
        const tocElement = document.getElementById('tableOfContents');
        if (!viewerElement || !path) return;

        // Hide welcome screen
        const welcomeScreen = document.getElementById('welcomeScreen');
        if (welcomeScreen) {
            welcomeScreen.style.display = 'none';
        }

        viewerElement.innerHTML = '<div class="loading">Loading document...</div>';
        if (tocElement) tocElement.innerHTML = '';

        try {
            // Ensure path is properly encoded and doesn't start with /
            const cleanPath = path.replace(/^\/+/, '').replace(/\\/g, '/');
            const encodedPath = encodeURIComponent(cleanPath).replace(/%2F/g, '/');
            
            const response = await fetch(`/api/documents/${encodedPath}`, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                }
            });

            if (!response.ok) {
                if (response.status === 404) {
                    throw new Error(`Document not found: ${cleanPath}`);
                }
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const doc = await response.json();
            if (!doc || !doc.content) {
                throw new Error('Invalid document format');
            }

            this.currentDocument = doc;
            this.renderDocument(doc);
        } catch (error) {
            console.error('Error loading document:', error);
            viewerElement.innerHTML = `
                <div class="error">
                    <p>Error loading document: ${this.escapeHtml(error.message)}</p>
                    <button class="btn-link" onclick="documentViewer.goHome()">Back to Home</button>
                </div>
            `;
        }
    }

    renderDocument(doc) {
        const viewerElement = document.getElementById('documentViewer');
        const tocElement = document.getElementById('tableOfContents');
        const welcomeScreen = document.getElementById('welcomeScreen');
        if (!viewerElement) return;

        // Hide welcome screen when showing a document
        if (welcomeScreen) {
            welcomeScreen.style.display = 'none';
        }

        // Render markdown using marked.js or fallback to custom parser
        let html;
        if (this.marked) {
            try {
                html = this.marked.parse(doc.content || '');
            } catch (error) {
                console.error('marked.js rendering failed, using fallback:', error);
                html = this.parseMarkdown(doc.content || '');
            }
        } else {
            html = this.parseMarkdown(doc.content || '');
        }

        viewerElement.innerHTML = `
            <div class="document-content">
                <div class="document-header">
                    <h1 class="document-title">${this.escapeHtml(doc.name)}</h1>
                    <div class="document-info">
                        <span>Last modified: ${this.formatDate(doc.lastModified)}</span>
                        <span>Type: ${this.escapeHtml(this.getDocumentType(doc.name))}</span>
                    </div>
                </div>
                <div class="markdown-content">
                    ${html}
                </div>
            </div>
        `;

        // Add anchor IDs to headers and render table of contents
        this.addHeaderAnchors();
        if (doc.toc && doc.toc.length > 0 && tocElement) {
            this.renderTableOfContents(doc.toc, tocElement);
        }
    }

    addHeaderAnchors() {
        const viewerElement = document.getElementById('documentViewer');
        if (!viewerElement) return;

        const headers = viewerElement.querySelectorAll('h1, h2, h3, h4, h5, h6');
        headers.forEach(header => {
            if (!header.id) {
                const text = header.textContent || '';
                const anchor = this.generateAnchorId(text);
                header.id = anchor;
                header.classList.add('header-anchor');
            }
        });
    }

    generateAnchorId(text) {
        if (!text) return 'header';

        return text.toLowerCase()
            .replace(/[^\w\s-]/g, '')
            .replace(/\s+/g, '-')
            .replace(/-+/g, '-')
            .replace(/^-+|-+$/g, '') || 'header';
    }

    renderTableOfContents(toc, tocElement) {
        if (!toc || toc.length === 0) {
            tocElement.innerHTML = '';
            return;
        }

        const tocHtml = `
            <div class="toc-header">
                <h3>Table of Contents</h3>
            </div>
            <nav class="toc-nav">
                ${toc.map(item => `
                    <a href="#${this.escapeHtml(item.anchor)}" class="toc-item toc-level-${item.level}">
                        ${this.escapeHtml(item.text)}
                    </a>
                `).join('')}
            </nav>
        `;

        tocElement.innerHTML = tocHtml;

        // Smooth scroll to anchors
        tocElement.querySelectorAll('a').forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const targetId = link.getAttribute('href').substring(1);
                const target = document.getElementById(targetId);
                if (target) {
                    target.scrollIntoView({ behavior: 'smooth', block: 'start' });
                    // Highlight the header briefly
                    target.classList.add('header-highlight');
                    setTimeout(() => target.classList.remove('header-highlight'), 2000);
                }
            });
        });
    }

    parseMarkdown(text) {
        if (!text) return '<p>No content</p>';

        let html = text;
        const lines = html.split('\n');

        // Process code blocks first (before other processing)
        html = html.replace(/```(\w+)?\n([\s\S]*?)```/g, (match, lang, code) => {
            const escaped = this.escapeHtml(code.trim());
            return `<pre><code${lang ? ` class="language-${lang}"` : ''}>${escaped}</code></pre>`;
        });

        // Process inline code (after code blocks)
        html = html.replace(/`([^`\n]+)`/g, '<code>$1</code>');

        // Headers (process from h6 to h1 to avoid conflicts)
        html = html.replace(/^###### (.*$)/gim, '<h6>$1</h6>');
        html = html.replace(/^##### (.*$)/gim, '<h5>$1</h5>');
        html = html.replace(/^#### (.*$)/gim, '<h4>$1</h4>');
        html = html.replace(/^### (.*$)/gim, '<h3>$1</h3>');
        html = html.replace(/^## (.*$)/gim, '<h2>$1</h2>');
        html = html.replace(/^# (.*$)/gim, '<h1>$1</h1>');

        // Horizontal rules
        html = html.replace(/^---$/gim, '<hr>');
        html = html.replace(/^\*\*\*$/gim, '<hr>');
        html = html.replace(/^___$/gim, '<hr>');

        // Blockquotes
        html = html.replace(/^> (.*$)/gim, '<blockquote>$1</blockquote>');
        // Merge consecutive blockquotes
        html = html.replace(/<\/blockquote>\n<blockquote>/g, '<br>');

        // Lists - ordered
        html = html.replace(/^(\d+)\. (.*$)/gim, '<li>$2</li>');
        // Lists - unordered
        html = html.replace(/^[\*\-+] (.*$)/gim, '<li>$1</li>');
        // Wrap consecutive list items
        html = html.replace(/(<li>.*<\/li>\n?)+/g, (match) => {
            // Check if it's ordered (has numbers) or unordered
            const isOrdered = /^\d+\./.test(match);
            return isOrdered ? `<ol>${match}</ol>` : `<ul>${match}</ul>`;
        });

        // Tables
        const tableRegex = /^\|(.+)\|$/gm;
        let tableMatch;
        const tables = [];
        while ((tableMatch = tableRegex.exec(html)) !== null) {
            const row = tableMatch[1].split('|').map(cell => cell.trim()).filter(cell => cell);
            if (row.length > 0) {
                const isHeader = html.substring(tableMatch.index - 10, tableMatch.index).includes('---');
                const tag = isHeader ? 'th' : 'td';
                tables.push({ index: tableMatch.index, row, tag, isHeader });
            }
        }
        // Process tables (simple pipe table support)
        html = html.replace(/^\|(.+)\|$/gm, (match, content) => {
            const cells = content.split('|').map(cell => cell.trim()).filter(cell => cell);
            if (cells.length === 0) return match;
            // Check if it's a separator row
            if (cells.every(cell => /^:?-+:?$/.test(cell))) {
                return ''; // Remove separator rows
            }
            return '<tr>' + cells.map(cell => `<td>${cell}</td>`).join('') + '</tr>';
        });
        // Wrap table rows in table
        html = html.replace(/(<tr>.*<\/tr>\n?)+/g, '<table>$&</table>');

        // Bold and italic (process bold first, then italic)
        html = html.replace(/\*\*\*(.*?)\*\*\*/g, '<strong><em>$1</em></strong>');
        html = html.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
        html = html.replace(/\*(.*?)\*/g, '<em>$1</em>');
        html = html.replace(/__(.*?)__/g, '<strong>$1</strong>');
        html = html.replace(/_(.*?)_/g, '<em>$1</em>');

        // Links
        html = html.replace(/\[([^\]]+)\]\(([^)]+)\)/g, '<a href="$2" target="_blank" rel="noopener noreferrer">$1</a>');

        // Images
        html = html.replace(/!\[([^\]]*)\]\(([^)]+)\)/g, '<img src="$2" alt="$1">');

        // Line breaks (double newline = paragraph, single newline = br)
        const paragraphs = html.split(/\n\n+/);
        html = paragraphs.map(para => {
            para = para.trim();
            if (!para) return '';
            // Don't wrap if it's already a block element
            if (/^<(h[1-6]|ul|ol|li|blockquote|pre|table|hr)/.test(para)) {
                return para;
            }
            // Convert single newlines to <br> within paragraphs
            para = para.replace(/\n/g, '<br>');
            return `<p>${para}</p>`;
        }).filter(p => p).join('\n');

        // Escape any remaining HTML that wasn't processed
        // But preserve the HTML we just created
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = html;
        // Re-escape text nodes that shouldn't be HTML
        const walker = document.createTreeWalker(tempDiv, NodeFilter.SHOW_TEXT, null, false);
        const textNodes = [];
        let node;
        while (node = walker.nextNode()) {
            if (node.parentElement && !['CODE', 'PRE', 'SCRIPT', 'STYLE'].includes(node.parentElement.tagName)) {
                textNodes.push(node);
            }
        }
        // Actually, we've already processed everything, so we can return as-is
        return html;
    }

    getDocumentType(fileName) {
        const parts = fileName.split('-');
        if (parts.length >= 2) {
            return parts[1].toUpperCase();
        }
        return 'DOC';
    }

    formatDate(dateString) {
        if (!dateString) return 'Unknown';

        try {
            const date = new Date(dateString);
            if (isNaN(date.getTime())) {
                return 'Invalid date';
            }
            return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        } catch (error) {
            return 'Invalid date';
        }
    }

    formatSize(bytes) {
        if (bytes < 1024) return bytes + ' B';
        if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
        return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    getWelcomeContent() {
        return `
            <div class="welcome-header">
                <h1>Documentation Toolkit</h1>
                <p>A comprehensive framework for generating professional documentation and building semantic knowledge bases for your projects.</p>
            </div>

            <div class="welcome-section">
                <h2>Getting Started</h2>
                <p>This web interface displays all markdown documents from your <code>docs/</code> folder. Navigate through the folder structure in the sidebar to view your documents.</p>
                
                <h3>Using the CLI Application</h3>
                <p>Generate documents using the command-line interface:</p>
                
                <div class="command-grid">
                    <div class="command-card">
                        <h4>📝 Generate Document</h4>
                        <code>doc generate &lt;type&gt; "Name"</code>
                        <p>Create a new document from a template. Types: prd, rfp, tender, sow, architecture, solution, spec, api, data, blog, weekly-log</p>
                    </div>
                    
                    <div class="command-card">
                        <h4>📁 Organize Documents</h4>
                        <code>doc gen prd "Name" --subfolder "folder"</code>
                        <p>Generate documents in subfolders to organize your documentation structure.</p>
                    </div>
                    
                    <div class="command-card">
                        <h4>🔍 Semantic Search</h4>
                        <code>doc search "query"</code>
                        <p>Search your project knowledge using semantic search after building an index.</p>
                    </div>
                    
                    <div class="command-card">
                        <h4>📊 Build Index</h4>
                        <code>doc index</code>
                        <p>Build a semantic index from your source files for intelligent search capabilities.</p>
                    </div>
                    
                    <div class="command-card">
                        <h4>🕸️ Knowledge Graph</h4>
                        <code>doc graph</code>
                        <p>Generate a knowledge graph showing relationships between entities and topics in your project.</p>
                    </div>
                    
                    <div class="command-card">
                        <h4>🌐 Web Interface</h4>
                        <code>doc web</code>
                        <p>Start this web server to view and share your documents. Use <code>--port</code> and <code>--host</code> options to customize.</p>
                    </div>
                </div>
            </div>

            <div class="welcome-section">
                <h2>Navigation</h2>
                <ul>
                    <li><strong>Sidebar:</strong> Browse your document folder structure. Click folders to expand/collapse, click documents to view them.</li>
                    <li><strong>Search:</strong> Use the search bar to find documents by content. Results show snippets with highlighted matches.</li>
                    <li><strong>Table of Contents:</strong> When viewing a document, the right sidebar shows an auto-generated table of contents for easy navigation.</li>
                    <li><strong>Theme Toggle:</strong> Switch between light and dark themes using the button in the header.</li>
                </ul>
            </div>

            <div class="welcome-section">
                <h2>Document Types</h2>
                <p>The toolkit supports 12+ document templates:</p>
                <ul>
                    <li><strong>PRD</strong> - Product Requirements Document</li>
                    <li><strong>RFP</strong> - Request for Proposal</li>
                    <li><strong>Tender</strong> - Tender Response</li>
                    <li><strong>SOW</strong> - Statement of Work</li>
                    <li><strong>Architecture</strong> - Architecture Design Document</li>
                    <li><strong>Solution</strong> - Solution Proposal</li>
                    <li><strong>Spec</strong> - Engineering Specification</li>
                    <li><strong>API</strong> - API Design Document</li>
                    <li><strong>Data</strong> - Data Model Documentation</li>
                    <li><strong>Blog</strong> - Blog Post Template</li>
                    <li><strong>Weekly Log</strong> - Weekly Progress Log</li>
                </ul>
            </div>

            <div class="welcome-section">
                <h2>Tips</h2>
                <ul>
                    <li>Documents are organized by date and type in the filename: <code>YYYY-MM-DD-type-name.md</code></li>
                    <li>Use subfolders to organize related documents (e.g., <code>architecture/</code>, <code>requirements/</code>)</li>
                    <li>The web interface automatically updates when you refresh the page or click the refresh button</li>
                    <li>All documents are served as raw markdown and rendered client-side for optimal performance</li>
                </ul>
            </div>

            <div class="welcome-section">
                <p style="text-align: center; color: var(--text-muted); font-size: 0.875rem; margin-top: 2rem;">
                    Select a document from the sidebar to start viewing, or generate new documents using the CLI commands above.
                </p>
            </div>
        `;
    }
}

// Global reference for inline event handlers
let documentViewer;

// Initialize the application when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        documentViewer = new DocumentViewer();
    });
} else {
    documentViewer = new DocumentViewer();
}
