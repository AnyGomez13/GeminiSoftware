import React from 'react';
import {
  Users,
  Dog,
  ClipboardList,
  Syringe,
  Bell,
  LogOut,
  ShieldCheck,
  Stethoscope
} from 'lucide-react';

export type NavTab = 'propietarios' | 'pacientes' | 'historias' | 'vacunacion' | 'recordatorios';

interface SidebarProps {
  activeTab: NavTab;
  onTabChange: (tab: NavTab) => void;
  onLogout?: () => void;
  usuarioNombre?: string;
  recordatoriosPendientesCount?: number;
}

export const Sidebar: React.FC<SidebarProps> = ({
  activeTab,
  onTabChange,
  onLogout,
  usuarioNombre = 'Administrador',
  recordatoriosPendientesCount = 0,
}) => {
  const navItems = [
    {
      id: 'propietarios' as NavTab,
      label: 'Propietarios',
      icon: Users,
      description: 'Directorio y Acudientes',
    },
    {
      id: 'pacientes' as NavTab,
      label: 'Pacientes',
      icon: Dog,
      description: 'Censo y Fichas Médicas',
    },
    {
      id: 'historias' as NavTab,
      label: 'Historia Clínica',
      icon: ClipboardList,
      description: 'Consultas Inmutables 360°',
    },
    {
      id: 'vacunacion' as NavTab,
      label: 'Inmunización',
      icon: Syringe,
      description: 'Biológicos y Carnet PDF',
    },
    {
      id: 'recordatorios' as NavTab,
      label: 'Recordatorios',
      icon: Bell,
      description: 'Refuerzos y WhatsApp 1-Clic',
      badge: recordatoriosPendientesCount > 0 ? recordatoriosPendientesCount : undefined,
    },
  ];

  return (
    <aside className="w-64 bg-botanical-forest text-white flex flex-col h-screen select-none shadow-xl flex-shrink-0 z-20">
      {/* Brand Header */}
      <div className="p-5 border-b border-white/10 flex items-center space-x-3">
        <div className="w-10 h-10 rounded-xl bg-botanical-emerald flex items-center justify-center shadow-md">
          <Stethoscope className="w-6 h-6 text-white" />
        </div>
        <div>
          <h1 className="font-bold text-lg leading-tight tracking-tight">VetClinic Pro</h1>
          <p className="text-xs text-white/70 font-medium">Dres. Fabio & William</p>
        </div>
      </div>

      {/* Navigation List */}
      <nav className="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
        <div className="px-3 pb-2 text-[11px] font-semibold tracking-wider text-white/50 uppercase">
          Gestión Clínica
        </div>

        {navItems.map((item) => {
          const Icon = item.icon;
          const isActive = activeTab === item.id;

          return (
            <button
              key={item.id}
              onClick={() => onTabChange(item.id)}
              className={`w-full flex items-center px-3 py-3 rounded-xl text-left transition-all duration-150 group relative ${
                isActive
                  ? 'bg-white/15 text-white font-medium shadow-sm'
                  : 'text-white/80 hover:bg-white/10 hover:text-white'
              }`}
            >
              <Icon
                className={`w-5 h-5 mr-3 transition-colors ${
                  isActive ? 'text-emerald-300' : 'text-white/70 group-hover:text-white'
                }`}
              />
              <div className="flex-1 min-w-0">
                <div className="text-sm font-medium leading-none">{item.label}</div>
                <div className="text-[11px] text-white/60 truncate mt-1">
                  {item.description}
                </div>
              </div>

              {item.badge !== undefined && (
                <span className="ml-2 px-2 py-0.5 text-xs font-bold rounded-full bg-botanical-amber text-botanical-graphite shadow-sm animate-pulse">
                  {item.badge}
                </span>
              )}

              {isActive && (
                <div className="absolute left-0 top-2 bottom-2 w-1 bg-emerald-400 rounded-r-full" />
              )}
            </button>
          );
        })}
      </nav>

      {/* Footer / User Station */}
      <div className="p-4 border-t border-white/10 bg-black/10">
        <div className="flex items-center justify-between mb-3">
          <div className="flex items-center space-x-2">
            <div className="w-8 h-8 rounded-full bg-white/20 flex items-center justify-center text-xs font-bold text-white">
              {usuarioNombre.charAt(0).toUpperCase()}
            </div>
            <div className="min-w-0">
              <div className="text-xs font-medium text-white truncate max-w-[110px]">
                {usuarioNombre}
              </div>
              <div className="flex items-center text-[10px] text-emerald-300">
                <ShieldCheck className="w-3 h-3 mr-1 inline" />
                Estación Local
              </div>
            </div>
          </div>

          {onLogout && (
            <button
              onClick={onLogout}
              title="Cerrar sesión"
              className="p-1.5 rounded-lg text-white/70 hover:text-white hover:bg-white/10 transition-colors"
            >
              <LogOut className="w-4 h-4" />
            </button>
          )}
        </div>

        <div className="text-[10px] text-center text-white/40 pt-1 border-t border-white/5">
          SQLite Monopuesto • Ley 576 de 2000
        </div>
      </div>
    </aside>
  );
};
