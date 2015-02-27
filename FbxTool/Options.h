
#pragma once

using namespace System;
using namespace Fusion;
using namespace msclr::interop;
using namespace Fusion::Graphics;

/*
**	Options class :
*/
public ref class Options {
	private:
	string^ input;
	string^ output;
	string^ base;
	float	mergeTolerance;
	bool	wait;
	bool	anim;
	bool	geom;

	public:

	Options() {
		input			=	nullptr;
		output			=	nullptr;
		base			=	nullptr;
		mergeTolerance	=	0;
		wait			=	false;
		geom			=	false;
	}

	[CmdLineParser::Name("in")]
	[CmdLineParser::Required()]
	property string^ Input { 
		void set(string^ value) { input = value; }
		string ^get() { return input; }
	}

	[CmdLineParser::Name("out")]
	property string^ Output { 
		void set(string^ value) { output = value; }
		string ^get() { return output; }
	}

	[CmdLineParser::Name("merge")]
	property float MergeTolerance { 
		void set(float value) { mergeTolerance = value; } 
		float get() { return mergeTolerance; } 
	}

	[CmdLineParser::Name("base")]
	property string ^BaseDirectory { 
		void set(string ^value) { base = value; } 
		string ^get() { return base; } 
	}

	[CmdLineParser::Name("anim")]
	property bool ImportAnimation {
		void set(bool value) { anim = value; } 
		bool get() { return anim; } 
	}

	[CmdLineParser::Name("geom")]
	property bool ImportGeometry {
		void set(bool value) { geom = value; } 
		bool get() { return geom; } 
	}

	[CmdLineParser::Name("wait")]
	property bool Wait {
		void set(bool value) { wait = value; } 
		bool get() { return wait; } 
	}
};




