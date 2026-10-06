import React, { useState } from 'react';
import { MainLayout } from './components/layout/MainLayout';
import type { NavTab } from './components/layout/Sidebar';
import {
  Users,
  Dog,
  Syringe,
  Bell,
  Stethoscope,
  ShieldCheck,
  ArrowRight,
  HeartPulse
} from 'lucide-react';
import { toast } from 'sonner';

export const App: React.FC = () => {
  const [activeTab, setActiveTab] = useState<NavTab>('pacientes');
  const [searchTerm, setSearchTerm] = useState('');
  const [usuario] = useState({
    nombreCompleto: 'Dr. Fabio / Dr. William',
    username: 'admin',
    rol: 'Administrador Clínico',
  });

  const getTabTitle = (tab: NavTab) => {
    switch (tab) {
      case 'propietarios':
        return { title: 'Directorio de Propietarios', subtitle: 'Gestión de acudientes y validación de contacto' };
      case 'pacientes':
        return { title: 'Censo de Pacientes', subtitle: 'Fichas clínicas 360°, especies y evolución de peso' };
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
    toast.info('Modal de registro rápido disponible en los módulos W-08 a W-10.', {
      description: 'El entorno de desarrollo frontend está listo y configurado.'
    });
  };

  return (
    <MainLayout
      activeTab={activeTab}
      onTabChange={setActiveTab}
      title={currentMeta.title}
      subtitle={currentMeta.subtitle}
      usuarioNombre={usuario.nombreCompleto}
      recordatoriosPendientesCount={2}
      searchValue={searchTerm}
      onSearchChange={setSearchTerm}
      onNewActionClick={handleQuickAction}
    >
      {/* Welcome Clinical Banner */}
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

      {/* KPI Stats Overview Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-white p-5 rounded-xl border border-botanical-stone shadow-card hover:shadow-card-hover transition-all">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-botanical-muted uppercase tracking-wider">
              Pacientes Activos
            </span>
            <div className="p-2 rounded-lg bg-emerald-50 text-botanical-emerald">
              <Dog className="w-5 h-5" />
            </div>
          </div>
          <div className="mt-3 flex items-baseline justify-between">
            <span className="text-2xl font-bold text-botanical-graphite">2</span>
            <span className="text-xs text-botanical-emerald font-medium">Maya & Osita</span>
          </div>
        </div>

        <div className="bg-white p-5 rounded-xl border border-botanical-stone shadow-card hover:shadow-card-hover transition-all">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-botanical-muted uppercase tracking-wider">
              Acudientes Registrados
            </span>
            <div className="p-2 rounded-lg bg-emerald-50 text-botanical-emerald">
              <Users className="w-5 h-5" />
            </div>
          </div>
          <div className="mt-3 flex items-baseline justify-between">
            <span className="text-2xl font-bold text-botanical-graphite">2</span>
            <span className="text-xs text-botanical-muted">Celular validado</span>
          </div>
        </div>

        <div className="bg-white p-5 rounded-xl border border-botanical-stone shadow-card hover:shadow-card-hover transition-all">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-botanical-muted uppercase tracking-wider">
              Refuerzos Próximos
            </span>
            <div className="p-2 rounded-lg bg-amber-50 text-botanical-amber">
              <Bell className="w-5 h-5" />
            </div>
          </div>
          <div className="mt-3 flex items-baseline justify-between">
            <span className="text-2xl font-bold text-botanical-graphite">1</span>
            <span className="text-xs text-botanical-amber font-semibold">1-Clic WhatsApp</span>
          </div>
        </div>

        <div className="bg-white p-5 rounded-xl border border-botanical-stone shadow-card hover:shadow-card-hover transition-all">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-botanical-muted uppercase tracking-wider">
              Historias Inmutables
            </span>
            <div className="p-2 rounded-lg bg-emerald-50 text-botanical-emerald">
              <HeartPulse className="w-5 h-5" />
            </div>
          </div>
          <div className="mt-3 flex items-baseline justify-between">
            <span className="text-2xl font-bold text-botanical-graphite">2</span>
            <span className="text-xs text-botanical-emerald font-medium">Ley 576 al día</span>
          </div>
        </div>
      </div>

      {/* Module Navigation Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
        <button
          onClick={() => setActiveTab('pacientes')}
          className="text-left bg-white p-5 rounded-xl border border-botanical-stone shadow-card hover:border-botanical-emerald hover:shadow-card-hover transition-all group"
        >
          <div className="w-10 h-10 rounded-lg bg-emerald-50 text-botanical-emerald flex items-center justify-center mb-4 group-hover:bg-botanical-emerald group-hover:text-white transition-colors">
            <Dog className="w-5 h-5" />
          </div>
          <h3 className="text-base font-bold text-botanical-graphite group-hover:text-botanical-forest transition-colors">
            Censo de Pacientes
          </h3>
          <p className="mt-1 text-xs text-botanical-muted">
            Consulta expedientes 360°, calcula edades automáticas y visualiza curvas de evolución de peso.
          </p>
          <div className="mt-4 flex items-center text-xs font-semibold text-botanical-emerald">
            <span>Acceder al censo</span>
            <ArrowRight className="w-4 h-4 ml-1 transform group-hover:translate-x-1 transition-transform" />
          </div>
        </button>

        <button
          onClick={() => setActiveTab('vacunacion')}
          className="text-left bg-white p-5 rounded-xl border border-botanical-stone shadow-card hover:border-botanical-emerald hover:shadow-card-hover transition-all group"
        >
          <div className="w-10 h-10 rounded-lg bg-emerald-50 text-botanical-emerald flex items-center justify-center mb-4 group-hover:bg-botanical-emerald group-hover:text-white transition-colors">
            <Syringe className="w-5 h-5" />
          </div>
          <h3 className="text-base font-bold text-botanical-graphite group-hover:text-botanical-forest transition-colors">
            Pasaporte y Carnet Digital
          </h3>
          <p className="mt-1 text-xs text-botanical-muted">
            Diseño botánico moderno de carnet con impresión directa y exportación a PDF legal de alta resolución.
          </p>
          <div className="mt-4 flex items-center text-xs font-semibold text-botanical-emerald">
            <span>Ver carnet digital</span>
            <ArrowRight className="w-4 h-4 ml-1 transform group-hover:translate-x-1 transition-transform" />
          </div>
        </button>

        <button
          onClick={() => setActiveTab('recordatorios')}
          className="text-left bg-white p-5 rounded-xl border border-botanical-stone shadow-card hover:border-botanical-amber hover:shadow-card-hover transition-all group"
        >
          <div className="w-10 h-10 rounded-lg bg-amber-50 text-botanical-amber flex items-center justify-center mb-4 group-hover:bg-botanical-amber group-hover:text-white transition-colors">
            <Bell className="w-5 h-5" />
          </div>
          <h3 className="text-base font-bold text-botanical-graphite group-hover:text-botanical-amberDark transition-colors">
            Recordatorios 1-Clic
          </h3>
          <p className="mt-1 text-xs text-botanical-muted">
            Monitorea refuerzos próximos y vencidos. Notifica por WhatsApp Web y correo a costo cero.
          </p>
          <div className="mt-4 flex items-center text-xs font-semibold text-botanical-amberDark">
            <span>Ver tablero</span>
            <ArrowRight className="w-4 h-4 ml-1 transform group-hover:translate-x-1 transition-transform" />
          </div>
        </button>
      </div>
    </MainLayout>
  );
};

export default App;
