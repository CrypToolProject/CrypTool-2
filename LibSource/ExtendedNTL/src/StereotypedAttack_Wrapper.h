#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Diagnostics;


namespace WinNTL {

	/// <summary>
	/// Zusammenfassung für CT1_Wrapper
	/// </summary>
	public ref class CT1_Wrapper :  public System::ComponentModel::Component
	{
	public:
		CT1_Wrapper(void)
		{
			InitializeComponent();
			//
			//TODO: Konstruktorcode hier hinzufügen.
			//
		}
		CT1_Wrapper(System::ComponentModel::IContainer ^container)
		{
			/// <summary>
			/// Erforderlich für die Unterstützung des Windows.Forms-Klassenkompositions-Designers
			/// </summary>

			container->Add(this);
			InitializeComponent();
		}

	protected:
		/// <summary>
		/// Verwendete Ressourcen bereinigen.
		/// </summary>
		~CT1_Wrapper()
		{
			if (components)
			{
				delete components;
			}
		}

	private:
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		System::ComponentModel::Container ^components;

#pragma region Windows Form Designer generated code
		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung.
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		void InitializeComponent(void)
		{
			components = gcnew System::ComponentModel::Container();
		}
#pragma endregion
	};
}
