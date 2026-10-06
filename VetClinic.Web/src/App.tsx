import React, { useState } from 'react';
import { AuthProvider, useAuth } from './context/AuthContext';
import { ProtectedRoute } from './components/auth/ProtectedRoute';
import { MainLayout } from './components/layout/MainLayout';
import type { NavTab } from './components/layout/Sidebar';
import { PropietariosPage } from './pages/PropietariosPage';
import { PacientesPage } from './pages/PacientesPage';
import {
  Stethoscope,
  ShieldCheck
} from 'lucide-react';
import { toast } from 'sonner';

const AppContent: React.FC = () => {
  const { usuario, logout } = useAuth();
  const [activeTab, setActiveTab] = useState<NavTab>('pacientes');
  const [searchTerm, setSearchTerm] = useState('');
  const [filterPropId, setFilterPropId] = useState<number | null>(null);
  const [selectedPacienteId, setSelectedPacienteId] = useState<number | null>(null);

  const getTabTitle = (tab: NavTab) => {
    switch (tab) {
      case 'propietarios':
        return { title: 'Directorio de Propietarios', subtitle: 'Gestión de acudientes y validación de celular Colombia (RN-04)' };
      case 'pacientes':
        return { title: 'Censo de Pacientes', subtitle: 'Fichas clínicas 360°, especies y evolución de peso (RN-05, RN-06)' };
      case 'historias':
        return { title: 'Historias Clínicas', subtitle: 'Registros cronológicos inmutables según Ley 576 de 2000' };
      case 'vacunacion':
        return { title: 'Plan de Inmunización y Carnet', subtitle: 'Control de biológicos y carnet digital imprimible' };
      case 'recordatorios':
        return { title: 'Tablero de Recordatorios', subtitle: 'Refuerzos pendientes con enlaces nativos de WhatsApp y correo' };
    }
  };

  const currentMeta = getTabTitle(activeTab);

  const handleQuickAction = () => {
    toast.info('Utilice los botones "+ Registrar" dentro de cada módulo para crear nuevos registros.');
  };

  const renderTabContent = () => {
    switch (activeTab) {
      case 'propietarios':
        return (
          <PropietariosPage
            onSelectPacienteTab={(propId) => {
              setFilterPropId(propId || null);
              setActiveTab('pacientes');
            }}
          />
        );
      case 'pacientes':
        return (
          <PacientesPage
            filterPropietarioId={filterPropId}
            onSelectHistoriaTab={(pacienteId) => {
              setSelectedPacienteId(pacienteId);
              setActiveTab('historias');
            }}
            onSelectVacunacionTab={(pacienteId) => {
              setSelectedPacienteId(pacienteId);
              setActiveTab('vacunacion');
            }}
          />
        );
      default:
        return (
          <div className="space-y-6">
            {/* Clinical Banner */}
            <div className="relative overflow-hidden rounded-2xl bg-gradient-to-r from-botanical-forest to-botanical-forestDark p-6 text-white shadow-card">
              <div className="relative z-10 max-w-2xl">
                <div className="inline-flex items-center space-x-1.5 px-3 py-1 rounded-full bg-white/10 text-emerald-300 text-xs font-medium mb-3 backdrop-blur-sm">
                  <ShieldCheck className="w-3.5 h-3.5" />
                  <span>Estación Monopuesto Local • SQLite Offline</span>
                </div>
                <h2 className="text-2xl font-bold tracking-tight">
                  Bienvenido a VetClinic Pro
                </h2>
                <p className="mt-1 text-sm text-white/80 leading-relaxed">
                  Sistema de gestión veterinaria con trazabilidad médica legal para los Dres. Fabio y William.
                  Historias clínicas inmutables, carnet digital y recordatorios a cero costo de mensajería.
                </p>
              </div>

              <div className="absolute right-0 bottom-0 translate-x-8 translate-y-8 opacity-10 pointer-events-none">
                <Stethoscope className="w-64 h-64 text-white" />
              </div>
            </div>

            {/* Placeholder info for upcoming modules */}
            <div className="bg-white rounded-2xl p-8 border border-botanical-stone text-center shadow-card">
              <h3 className="text-base font-bold text-botanical-graphite mb-2">
                Módulo en Preparación ({currentMeta.title})
              </h3>
              <p className="text-xs text-botanical-muted max-w-md mx-auto">
                {selectedPacienteId
                  ? `Paciente seleccionado ID: ${selectedPacienteId}. Conectando en la siguiente fase de desarrollo.`
                  : 'Navega al Censo de Pacientes o Directorio de Propietarios para consultar o registrar información.'}
              </p>
              <div className="mt-4 flex justify-center gap-3">
                <button
                  onClick={() => setActiveTab('pacientes')}
                  className="px-4 py-2 bg-botanical-emerald text-white rounded-xl text-xs font-semibold hover:bg-botanical-emeraldHover transition-all"
                >
                  Ir al Censo de Pacientes
                </button>
                <button
                  onClick={() => setActiveTab('propietarios')}
                  className="px-4 py-2 bg-botanical-forest text-white rounded-xl text-xs font-semibold hover:bg-botanical-forestDark transition-all"
                >
                  Ir al Directorio de Tutores
                </button>
              </div>
            </div>
          </div>
        );
    }
  };

  return (
    <MainLayout
      activeTab={activeTab}
      onTabChange={(tab) => {
        if (tab === 'pacientes' && activeTab !== 'pacientes') {
          setFilterPropId(null);
        }
        setActiveTab(tab);
      }}
      title={currentMeta.title}
      subtitle={currentMeta.subtitle}
      usuarioNombre={usuario?.nombreCompleto || 'Dr. Fabio / Dr. William'}
      onLogout={logout}
      recordatoriosPendientesCount={2}
      searchValue={searchTerm}
      onSearchChange={setSearchTerm}
      onNewActionClick={handleQuickAction}
    >
      {renderTabContent()}
    </MainLayout>
  );
};

export const App: React.FC = () => {
  return (
    <AuthProvider>
      <ProtectedRoute>
        <AppContent />
      </ProtectedRoute>
    </AuthProvider>
  );
};

export default App;
