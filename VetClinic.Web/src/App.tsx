import React, { useState, useEffect } from 'react';
import { AuthProvider, useAuth } from './context/AuthContext';
import { ProtectedRoute } from './components/auth/ProtectedRoute';
import { MainLayout } from './components/layout/MainLayout';
import type { NavTab } from './components/layout/Sidebar';
import { PropietariosPage } from './pages/PropietariosPage';
import { PacientesPage } from './pages/PacientesPage';
import { HistoriaClinicaPage } from './pages/HistoriaClinicaPage';
import { VacunacionPage } from './pages/VacunacionPage';
import { RecordatoriosPage } from './pages/RecordatoriosPage';
import { api } from './services/apiClient';
import { toast } from 'sonner';

const AppContent: React.FC = () => {
  const { usuario, logout } = useAuth();
  const [activeTab, setActiveTab] = useState<NavTab>('pacientes');
  const [searchTerm, setSearchTerm] = useState('');
  const [filterPropId, setFilterPropId] = useState<number | null>(null);
  const [selectedPacienteId, setSelectedPacienteId] = useState<number | null>(null);
  const [recordatoriosCount, setRecordatoriosCount] = useState<number>(0);

  // Cargar contador de recordatorios activos para el badge del Sidebar
  useEffect(() => {
    const loadRemindersCount = async () => {
      try {
        const recs = await api.recordatorios.get(30, 'todos');
        setRecordatoriosCount(recs.length);
      } catch {
        // Ignorar fallo secundario
      }
    };
    loadRemindersCount();
  }, [activeTab]);

  const getTabTitle = (tab: NavTab) => {
    switch (tab) {
      case 'propietarios':
        return {
          title: 'Directorio de Propietarios',
          subtitle: 'Gestión de acudientes y validación estricta de celular Colombia (RN-04)'
        };
      case 'pacientes':
        return {
          title: 'Censo de Pacientes',
          subtitle: 'Fichas clínicas 360°, especies y evolución de peso (RN-05, RN-06)'
        };
      case 'historias':
        return {
          title: 'Historias Clínicas',
          subtitle: 'Registros cronológicos inmutables según Ley 576 de 2000 (RN-02, RN-07)'
        };
      case 'vacunacion':
        return {
          title: 'Plan de Inmunización y Carnet',
          subtitle: 'Control de biológicos y carnet digital imprimible (RN-08, RNF-06)'
        };
      case 'recordatorios':
        return {
          title: 'Tablero de Recordatorios',
          subtitle: 'Refuerzos pendientes con enlaces nativos de WhatsApp y correo a costo cero (RN-09)'
        };
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
      case 'historias':
        return (
          <HistoriaClinicaPage
            initialPacienteId={selectedPacienteId}
            onGoToVacunacion={(pacienteId) => {
              setSelectedPacienteId(pacienteId);
              setActiveTab('vacunacion');
            }}
          />
        );
      case 'vacunacion':
        return (
          <VacunacionPage
            initialPacienteId={selectedPacienteId}
          />
        );
      case 'recordatorios':
        return (
          <RecordatoriosPage
            onGoToVacunacion={(pacienteId) => {
              setSelectedPacienteId(pacienteId);
              setActiveTab('vacunacion');
            }}
          />
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
      recordatoriosPendientesCount={recordatoriosCount}
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
