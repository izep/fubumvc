﻿/** @jsx React.DOM */
var AssemblySummaryItem = React.createClass({
	render: function(){
		var url = '#structuremap/assembly/' + this.props.name;

		return (
		  <li className="list-group-item">
			<span className="badge">{this.props.count}</span>
			<b><a href={url}>{this.props.name}</a></b>
		  </li>
		);
	}
});

var NamespaceSummaryItem = React.createClass({
	render: function(){
	  var url = '#structuremap/namespace/' + this.props.name;
	
	  return (
		  <li className="list-group-item">
			<span className="badge">{this.props.count}</span>
			<a href={url}>{this.props.name}</a>
		  </li>
	  );
	}
});


function StructureMapSearch(){
	var self = this;
	
	self.options = null;
	
	self.find = function(query){
		query = query.toLowerCase();
	
		var matches = _.filter(self.options, function(o){
			try {
				return o.display.toLowerCase().indexOf(query) > -1;
			}
			catch (e){
				return false;
			}
		});

		return matches;
	}
	
	self.findMatches = function(query, callback){
		if (self.options == null){
			FubuDiagnostics.get('StructureMap:search_options', {}, function(data){
				self.options = data;
				callback(self.find(query));
			});
		}
		else{
			callback(self.find(query));
		}
	}
	
	return self;
}


FubuDiagnostics.StructureMapSearch = new StructureMapSearch();


var SearchBox = React.createClass({
	componentDidMount: function(){
	
		var element = this.getDOMNode();
		$(element).typeahead({
		  minLength: 5,
		  highlight: true
		},
		{
		  name: 'structuremap',
		  displayKey: 'value',
		  
		  source: FubuDiagnostics.StructureMapSearch.findMatches,
		  templates: {
			empty: '<div class="empty-message">No matches found</div>',
			suggestion: _.template('<div><p><%= display%> - <small><%= type%></small></p><p><small><%= value%></small></p></div>')
		  }
		});
		
		$(element).on('typeahead:selected', function(jquery, option){
			var url = 'structuremap/' + option.type + '/' + option.value;
			FubuDiagnostics.navigateTo(url);
		});
	},
	
	componentWillUnmount: function(){
		var element = this.getDOMNode();
		$(element).typeahead('destroy');
	},

	render: function(){
		return (
			<input type="search" name="search" ref="input" className="form-control typeahead" placeholder="Search the application container" />
		);
	}
});



var Summary = React.createClass({
	render: function(){
		var items = [];
		
		this.props.assemblies.forEach(function(assem, i){		
			items.push(AssemblySummaryItem(assem));
			
			assem.namespaces.forEach(function(ns){
				items.push(NamespaceSummaryItem(ns));
			});
			
		});
		
		return (
			<div>

			<SearchBox />
			
			<ul className="list-group">
				{items}
			</ul>
			
			</div>
		);
	}
});

FubuDiagnostics.addSection({
    title: 'StructureMap',
    description: 'Insight into the configuration and state of the application container',
    key: 'structuremap',
	screen: new ReactScreen({
		component: Summary,
		route: 'StructureMap:summary'
	})
});

