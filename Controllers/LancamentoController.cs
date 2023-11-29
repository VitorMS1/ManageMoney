using managemoney.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using managemoney.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
using managemoney.Models.ViewModels.Lancamento;
using Microsoft.AspNetCore.Mvc.Rendering;
using ManageMoney.Constantes;
using managemoney.Filters;
using managemoney.Extensions;

namespace managemoney.Controllers
{
    [Route("[controller]")]
    [Authorize]
    public class LancamentoController : Controller
    {
        private IMapper _mapper;
        private ILancamentoRepository _lancamentoRepository;
        private IUsuarioRepository _usuarioRepository;
        private ICategoriaRepository _categoriaRepository;

        public LancamentoController(IMapper mapper,
                                    ILancamentoRepository lancamentoRepository,
                                    IUsuarioRepository usuarioRepository,
                                    ICategoriaRepository categoriaRepository)
        {
            _lancamentoRepository = lancamentoRepository;
            _usuarioRepository = usuarioRepository;
            _mapper = mapper;
            _categoriaRepository = categoriaRepository;
        }

        [HttpGet("lancamentos")]
        [CustomActionFilter(Order = 5)]
        public IActionResult Lancamentos()
        {
            var lancamentos = _lancamentoRepository.ObterTodos();
            var lancamentoView = new LancamentosViewModel(lancamentos);
            return View(lancamentoView);
        }

        [HttpGet("cadastrarLancamento")]
        public IActionResult CadastrarLancamento()
        {
            return View(ObterLancamentoComCategorias());
        }

        [HttpGet("detalhesLancamento/{id}")]
        public IActionResult DetalhesLancamento(int id)
        {
            try
            {
                var lancamento = _lancamentoRepository.ObterPorId(id);
                var lancamentosView = _mapper.Map<DetalhesViewModel>(lancamento);
                return View(lancamentosView);
            }
            catch (Exception)
            {
                return View(ConstantesDasViews.ViewLancamentos);
            }
        }

        [HttpPost("cadastrarLancamento")]
        public ActionResult CadastrarLancamento([FromForm] CadastroLancamentoViewModel lancamento)
        {
            var model = ObterLancamentoComCategorias();
            try
            {
                if (ModelState.IsValid)
                {
                    _lancamentoRepository.Criar(_mapper.Map<LancamentoModel>(lancamento));
                    this.MostrarMensagem("Lançamento cadastrado com sucesso");
                    ModelState.Clear();
                    return View(ConstantesDasViews.ViewCadastrarLancamento, model);
                }
                else
                {
                    this.MostrarMensagem("Verifique se os campos obrigatórios estão preenchidos.", true);
                    return View(ConstantesDasViews.ViewCadastrarLancamento, model);
                }
            }
            catch (Exception) 
            {
                this.MostrarMensagem("Erro ao cadastrar lançamento! Tente novamente.", true);
                return View(ConstantesDasViews.ViewCadastrarLancamento, model);
            }
        }

        [HttpGet("atualizar/{id}")]
        public IActionResult Atualizar(int id)
        {
            try
            {
                var lancamento = _lancamentoRepository.ObterPorId(id);
                var lancamentoView = _mapper.Map<CadastroLancamentoViewModel>(lancamento);
                return View(ConstantesDasViews.ViewCadastrarLancamento, ObterLancamentoComCategorias(lancamentoView));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(ConstantesDasViews.ViewCadastrarLancamento);
            }
        }

        [HttpPost("atualizar/{id}")]
        public IActionResult Atualizar(int id, [FromForm] CadastroLancamentoViewModel novoLancamento)
        {
            try
            {
                _lancamentoRepository.Atualizar(id, _mapper.Map<LancamentoModel>(novoLancamento));
                var lancamento = _mapper.Map<CadastroLancamentoViewModel>(_lancamentoRepository.ObterPorId(id));

                if (ModelState.IsValid)
                {
                    this.MostrarMensagem("Lançamento atualizado com sucesso");
                    return View(ConstantesDasViews.ViewCadastrarLancamento, ObterLancamentoComCategorias(lancamento));
                }
                else
                {
                    return View(ConstantesDasViews.ViewCadastrarLancamento, ObterLancamentoComCategorias(lancamento));
                }
            }
            catch (Exception ex)
            {
                this.MostrarMensagem("Ocorreu um erro ao atualizar o lançamento: " + ex.Message, true);
                return View(ConstantesDasViews.ViewCadastrarLancamento, ObterLancamentoComCategorias(novoLancamento));
            }
        }

        [HttpGet("remover/{id}")]
        public ActionResult Remover(int id)
        {
            try
            {
                _lancamentoRepository.Remover(id);
                var lancamentos = _mapper.Map<LancamentosViewModel>(_lancamentoRepository.ObterTodos());
                return View(ConstantesDasViews.ViewLancamentos, lancamentos);
            }
            catch (Exception)
            {
                var lancamento = _lancamentoRepository.ObterPorId(id);
                var lancamentosView = _mapper.Map<DetalhesViewModel>(lancamento);
                return View(ConstantesDasViews.ViewDetalhesLancamento, lancamentosView);
            }
        }

        public CadastroLancamentoViewModel ObterLancamentoComCategorias(CadastroLancamentoViewModel cadastroLancamentoViewModel = null)
        {
            var viewCadLancamento = cadastroLancamentoViewModel ?? new CadastroLancamentoViewModel();
            foreach (var categoria in _categoriaRepository.ObterTodos())
            {
                if (viewCadLancamento.CategoriaID != categoria.Id)
                    viewCadLancamento.Categorias.Add(new SelectListItem
                    {
                        Text = categoria.Nome,
                        Value = categoria.Id.ToString()
                    });
            }

            return viewCadLancamento;
        }
    }
}