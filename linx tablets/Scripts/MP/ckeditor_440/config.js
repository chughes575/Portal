/**
 * @license Copyright (c) 2003-2013, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.html or http://ckeditor.com/license
 */

CKEDITOR.editorConfig = function( config ) {
	// Define changes to default configuration here. For example:
	config.height = '275';
	config.width = '660';
//	config.enterMode = CKEDITOR.ENTER_BR;

	config.toolbar = 'Min_Toolbar';
	config.toolbar_Min_Toolbar =
	[
		{ name: 'basicstyles', items : [ 'Format','Bold','Italic','Underline','Strike','Subscript','Superscript','-','RemoveFormat'  ] },
		{ name: 'paragraph', items : [ 'NumberedList','BulletedList','Table' ] },
		{ name: 'clipboard', items: ['Paste', 'PasteText', 'PasteFromWord', '-', 'Undo', 'Redo'] },
		{ name: 'editing', items : [ 'Find','Replace','-','SelectAll','-','SpellChecker' ] }
	];

	config.toolbar = 'Min_Toolbar_Admin';
	config.toolbar_Min_Toolbar_Admin =
	[
		{ name: 'basicstyles', items : [ 'Source','-','Format','Bold','Italic','Underline','Strike','Subscript','Superscript','-','RemoveFormat'  ] },
		{ name: 'paragraph', items : [ 'NumberedList','BulletedList','Table' ] },
		{ name: 'clipboard', items : [ 'Paste','PasteText','PasteFromWord','-','Undo','Redo' ] },
		{ name: 'editing', items : [ 'Find','Replace','-','SelectAll','-','SpellChecker' ] },
	];
};
